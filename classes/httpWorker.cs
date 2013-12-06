using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace NMCB_Launcher.classes
{
    class downloadItem
    {
        public string source;
        public string destination;

        public downloadItem(string _source, string _destination)
        {
            source = _source;
            destination = _destination;
        }
    }

    // used template:   http://stackoverflow.com/questions/6992553/how-do-i-async-download-multiple-files-using-webclient-but-one-at-a-time
    class httpWorker
    {
        public ProgressBar pb;
        public ProgressBar pbOverall;
        public RichTextBox rtbDebug;
        public Label lAct;
        public bool busy = false;
        private Queue<string> _items = new Queue<string>();
        private List<string> _results = new List<string>();
        NetworkCredential htaccess;

        private Queue<downloadItem> _downloadUrls = new Queue<downloadItem>();
        
        public httpWorker(ProgressBar _pb, ProgressBar _pbOverall, Label _lActual, RichTextBox _rtbDebug)
        {
            pb = _pb;
            pbOverall = _pbOverall;
            lAct = _lActual;
            rtbDebug = _rtbDebug;
            securityWorker sec = new securityWorker();
            htaccess = sec.getAccess();
        }

        public void startDownload()
        {
            pbOverall.Maximum = _downloadUrls.Count;
            pbOverall.Value = 0;

            pb.Maximum = 100;

            DownloadFiles();
        }

        public void addToDownload(downloadItem downloadSet)
        {
            _downloadUrls.Enqueue(downloadSet);

            // Starts the download
            
        }

        private void DownloadFiles()
        {
            if (_downloadUrls.Any())
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += client_DownloadProgressChanged;
                client.DownloadFileCompleted += client_DownloadFileCompleted;

                var url = _downloadUrls.Dequeue();
                INIWorker ini = new INIWorker();
                string modBase = ini.getModBase();
                if (url.source.Contains(modBase))
                    client.Credentials = htaccess;

                if (File.Exists(url.destination))
                    File.Delete(url.destination);

                client.DownloadFileAsync(new Uri(url.source), url.destination);
                rtbDebug.addLine(" dl: " + url.source + "\r\n     => " + url.destination);
                lAct.Text = url.source;
                return;
            }
            else
            {
                rtbDebug.addLine("fertsch");
            }

            // End of the download
            //btnGetDownload.Text = "Download Complete";
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // handle error scenario
                throw e.Error;
            }
            if (e.Cancelled)
            {
                // handle cancelled scenario
            }
            pbOverall.Value++;
            if (_downloadUrls.Count == 0)
                lAct.Text = "fertig.";
            DownloadFiles();
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            int pc = int.Parse(Math.Truncate(percentage).ToString());
            pb.Value = pc >= 0 ? pc : 0;
        }

        public int Count()
        {
            return _downloadUrls.Count;
        }

        public string getFileNameFormHeader(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.Credentials = htaccess;
                using (Stream rawStream = client.OpenRead(url))
                {
                    string fileName = string.Empty;
                    string contentDisposition = client.ResponseHeaders["content-disposition"];
                    if (!string.IsNullOrEmpty(contentDisposition))
                    {
                        string lookFor = "filename=";
                        int index = contentDisposition.IndexOf(lookFor, StringComparison.CurrentCultureIgnoreCase);
                        if (index >= 0)
                            fileName = contentDisposition.Substring(index + lookFor.Length);
                    }

                    rawStream.Close();

                    return fileName;

                    
                }
            }
        }
        
    }
}
