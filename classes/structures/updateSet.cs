using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;


namespace NMCB_Launcher.classes.structures
{

    class parallelOutputExists
    {
        public int version;
        public bool exists;

        public parallelOutputExists(int _version, bool _exists)
        {
            version = _version;
            exists = _exists;
        }
    }

    class parallelOutputData
    {
        public int version;
        public string stringData;

        public parallelOutputData(int _version, string _stringData)
        {
            version = _version;
            stringData = _stringData;
        }
    }

    class updateSet
    {
        public List<updateItem> data = new List<updateItem>(); // Note/Name, old, new, uploadFrom

        public int ftpVersion;
        public int dataVersion;
        public INIWorker localConfig = new INIWorker();
        NetworkCredential htaccess;

        public httpWorker http;
        string rootPathParallel = "";

        private BlockingCollection<int> versionsToCheck = new BlockingCollection<int>();
        private BlockingCollection<int> versionsToDownload = new BlockingCollection<int>();
        private BlockingCollection<int> versionsToLoad = new BlockingCollection<int>();
        private BlockingCollection<parallelOutputData> outputData = new BlockingCollection<parallelOutputData>();

        public updateSet()
        {
            securityWorker sec = new securityWorker();
            htaccess = sec.getAccess();
        }

        public void trim()
        {
            for (int i = 0; i <= data.Count - 1; i++)
            {
                data[i].pathOld = (@"\" + data[i].pathOld).Replace(@"\\", @"\").Replace(@"\\", @"\");
                data[i].pathNew = (@"\" + data[i].pathNew).Replace(@"\\", @"\").Replace(@"\\", @"\");
            }

            for (int i = 0; i <= data.Count - 1; i++)
            {
                for (int j = i; j <= data.Count - 1; j++)
                {
                    if (data[i].pathNew == data[j].pathOld && !data[i].isCleared() && !data[j].isCleared() && (data[i].pathNew != "" && data[i].pathNew != " " && data[i].pathNew != "  " && data[i].pathNew != @"\"))
                    {
                        data[i].pathNew = data[j].pathNew;
                        data[i].URL = data[j].URL;
                        data[j].clear();
                    }
                }
            }

            List<updateItem> newData = new List<updateItem>();

            for (int i = 0; i <= data.Count - 1; i++)
            {
                if (!data[i].isCleared())
                {
                    newData.Add(data[i]);
                }
            }

            data = newData;
        }

        public void addStringToData(string line)
        {
            if (line == "") return;

            string[] datas = line.Split(';');
            data.Add(new updateItem(datas[0], datas[1], datas[2], "", datas[3]));
        }
        public void writeFile(string path)
        {
            string dat = "";
            foreach (updateItem item in data)
            {
                dat += item.name + ";" + item.pathOld + ";" + item.pathNew + ";" + item.URL + "\r\n";
            }

            File.WriteAllText(path, dat);
        }

        #region Download
        public void _fileExistsPrallel()
        {
            int version;
            while (versionsToCheck.TryTake(out version))
            {
                HttpWebResponse response = null;
                var request = (HttpWebRequest)WebRequest.Create(new Uri(localConfig.getModBase() + rootPathParallel + ".version/" + (version).ToString()));
                request.Credentials = htaccess;
                request.Method = "HEAD";

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    versionsToLoad.TryAdd(version);
                    versionsToDownload.TryAdd(version);
                }
                catch //(WebException e)
                {
                    /* A WebException will be thrown if the status of the response is not `200 OK` */
                    versionsToLoad.TryAdd(-5);
                }
                finally
                {
                    // Don't forget to close your response.
                    if (response != null)
                    {
                        response.Close();
                    }
                }
            }
        }

        private void _getRemoteVersionParallel()
        {
            int i;

            while (versionsToDownload.TryTake(out i))
            {
                WebClient wc = new WebClient();
                wc.Credentials = htaccess;

                string d = wc.DownloadString(localConfig.getModBase() + rootPathParallel + @".version\" + i.ToString());
                outputData.TryAdd(new parallelOutputData(i, d));
                wc.Dispose();
            }
        }

        public void _getRemoteVersion(int startVersion, string rootPath)
        {
            rootPathParallel = rootPath;
            bool notFNF = true;
            int startSearchVersion = startVersion + 1;
            int verMax = startVersion;
            int parallelity = 11;
            data.Clear();

            while (notFNF)
            {
                for (int i = startSearchVersion; i < startSearchVersion + parallelity; i++)
                {
                    versionsToCheck.Add(i);
                    //Debug.WriteLine(i);
                }

                for (int k = 0; k <= 8; k++)
                    new Thread(_fileExistsPrallel).Start();

                while (versionsToLoad.Count < parallelity)
                    Application.DoEvents();
                
                int act;
                while (versionsToLoad.TryTake(out act))
                {
                    if (act != -5)
                    {
                        verMax = Math.Max(verMax, act);
                    }
                    else
                        notFNF = false;
                }

                startSearchVersion += parallelity;
            }

            for (int k = 0; k <= 20; k++)
                new Thread(_getRemoteVersionParallel).Start();

            while (outputData.Count < (verMax - startVersion))
                Application.DoEvents();

            List<string> sets = new List<string>();

            for (int i = 1; i <= (verMax - (startVersion + 1) + 1); i++)
                sets.Add("");

            parallelOutputData item;
            while (outputData.TryTake(out item))
            {
                sets[item.version - (startVersion + 1)] = item.stringData;
            }

            for (int i = 0; i < sets.Count(); i++)
            {
                string[] lines = sets[i].Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                foreach (string line in lines)
                {
                    addStringToData(line);
                }
            }           

            dataVersion = verMax;
        }

        public void _download(string rootRemote, string rootLocal)
        {
            foreach (updateItem item in data)
            {
                string pathNew = item.pathNew;
                string pathOld = item.pathOld;
                string localPath = "";
                string remotePath = "";

                if (item.URL.isURL())
                {
                    remotePath = item.URL;
                    WebClient wc = new WebClient();
                    wc.Credentials = htaccess;
                    if (remotePath.Contains("mediafire"))
                    {

                        string html = wc.DownloadString(remotePath);

                        int findDownload = html.IndexOf("http://download");
                        string preDownload = html.Substring(findDownload);

                        int findEnd = preDownload.IndexOf("\"");
                        remotePath = preDownload.Substring(0, findEnd);
                    }
                    else if (remotePath.Contains("dropbox") && !remotePath.Contains("https://dl.dropboxusercontent.com"))
                    {
                        string html = wc.DownloadString(remotePath);

                        int findDownload = html.IndexOf("https://dl.dropboxusercontent.com");
                        string preDownload = html.Substring(findDownload);

                        int findEnd = preDownload.IndexOf("\"");
                        remotePath = preDownload.Substring(0, findEnd);
                    }
                }
                else
                {
                    remotePath = localConfig.getModBase() + rootRemote + item.pathNew.Replace(@"\", "/");
                }

                localPath = rootLocal + item.pathNew;

                if (item.pathNew == "\\")
                {
                    localPath = rootLocal + item.pathOld;
                    if (File.Exists(localPath))
                        File.Delete(localPath);
                }
                else if ((localPath[localPath.Length - 1]) == '\\')
                {
                    if (!Directory.Exists(localPath))
                        Directory.CreateDirectory(localPath);
                }
                else
                {
                    //Debug.WriteLine("DL: " + remotePath);
                    //Debug.WriteLine("LOC:" + localPath);
                    http.addToDownload(new downloadItem(remotePath, localPath));

                }
            }
        }

        public bool _gotNewRemoteVersion(int oldVersion, string rootPath)
        {
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(new Uri(localConfig.getModBase() + rootPath + ".version/" + (oldVersion + 1).ToString()));
            request.Credentials = htaccess;
            request.Method = "HEAD";

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                return true;
            }
            catch //(WebException e)
            {
                /* A WebException will be thrown if the status of the response is not `200 OK` */
                return false;
            }
            finally
            {
                // Don't forget to close your response.
                if (response != null)
                {
                    response.Close();
                }
            }
        }
        #endregion

        #region Upload
        public bool _upload(FTPworker ftp, string root)
        {
            if (data.Count > 0)
            {
                string pathVer = @"ul\.ver_" + root.Replace("/", "");
                writeFile(pathVer);


                foreach (updateItem item in data)
                {
                    if (item.pathOld != "" && item.pathOld != @"\")
                    {
                        item.pathOld = item.pathOld.Replace(@"\", "/");

                        if (item.pathOld[item.pathOld.Length - 1] == '/')
                            deleteRecursive(ftp, root + item.pathOld);
                        else
                            ftp.FtpDelete(root + item.pathOld);
                    }

                    if (item.pathNew != "")
                    {
                        if (!item.URL.isURL())
                        {
                            Debug.WriteLine(root + item.pathNew.Replace(@"\", "/"));
                            if (!Directory.Exists(item.pathUploadFrom))
                                ftp.addToUpload(new uploadItem(item.pathUploadFrom, root + item.pathNew.Replace(@"\", "/")));
                            else
                                ftp.FtpCreateDirectory(root + item.pathNew.Replace(@"\", "/"));
                        }
                        else
                        {
                            string f = Directory.GetCurrentDirectory() + @"\ul\" + Path.GetFileName(item.pathNew);
                            File.WriteAllText(f, "");
                            //Debug.WriteLine(root + item.folderForURL + f);
                            ftp.addToUpload(new uploadItem(f, (root + item.pathNew).Replace(@"\", "/"), true));
                        }
                    }
                }

                ftp.Upload(Directory.GetCurrentDirectory() + @"\" + pathVer, root + ".version/" + (ftpVersion + 1).ToString());
                Debug.WriteLine(Directory.GetCurrentDirectory());

                data.Clear();
                return true;
            }
            return false;
        }

        private void addItem(string name, string pathOld, string pathNew, string pathUploadFrom, string _URL)
        {
            pathOld = @"\" + pathOld;
            pathNew = @"\" + pathNew;
            pathOld = pathOld.Replace(@"\\\", @"\").Replace(@"\\", @"\");
            pathNew = pathNew.Replace(@"\\\", @"\").Replace(@"\\", @"\");
            data.Add(new updateItem(name, pathOld, pathNew, pathUploadFrom, _URL));
        }

        public void deleteRecursive(FTPworker ftp, string path)
        {
            FTPdirectory dir = ftp.ListDirectoryDetail(path);
            FTPdirectory subdirs = dir.GetDirectories();
            FTPdirectory files = dir.GetFiles();

            foreach (FTPfileInfo subdir in subdirs)
            {
                deleteRecursive(ftp, subdir.FullName);
            }

            foreach (FTPfileInfo file in files)
            {
                string del = "";
                if (file.Extension == "")
                {
                    del = file.Path + "/" + file.NameOnly;
                }
                else
                {
                    del = file.Path + "/" + file.NameOnly + "." + file.Extension;
                }

                ftp.FtpDelete(del);
                Debug.WriteLine(del);
            }

            ftp.FtpDeleteDirectory(path);
        }
        public int _getVersion(FTPworker ftp, string root)
        {
            FTPdirectory versionFolder = ftp.ListDirectoryDetail(root + ".version/");
            FTPdirectory versionFiles = versionFolder.GetFiles();

            int i = -1;

            foreach (FTPfileInfo version in versionFiles)
            {
                i = Math.Max(i, Convert.ToInt16(version.NameOnly));
            }

            ftpVersion = i;

            return i;
        }

        public void addModOrConfig(string _path, string _folder)
        {
            pathCheck path = new pathCheck(_path);
            string URL = _path.isURL() ? _path : "";
            if (_path.isURL())
            {
                addItem("", "", _folder + @"\" + path.path, path.pathUL, URL);
            }
            else
            {
                addItem("", "", _folder + path.path, path.pathUL, "");
            }
        }
        public void addMapOrOptional(string name, string _path, string _folder)
        {
            pathCheck path = new pathCheck(_path);
            string URL = _path.isURL() ? _path : "";
            addItem(name, "", _folder + @"\" + path.path, path.pathUL, URL);
        }
        public void delete(string _path)
        {
            addItem("", _path, "", "", "");
        }
        public void replaceModOrConfig(string pathOld, string _path, string _URL, string ULPath = "")
        {
            pathCheck path = new pathCheck(_path);
            string URL = _path.isURL() ? _path : "";
            addItem("", pathOld, Path.GetDirectoryName(pathOld) + @"\" + path.path, path.pathUL, _URL);
        }
        #endregion
    }

    class updateItem
    {
        public string name;
        public string pathOld;
        public string pathNew;
        public string pathUploadFrom;
        public string URL;

        public updateItem(string _name, string _pathOld, string _pathNew, string _pathUploadFrom, string _URL)
        {
            name = _name;
            pathOld = _pathOld;
            pathNew = _pathNew;
            pathUploadFrom = _pathUploadFrom;
            URL = _URL;
        }

        public bool isCleared()
        {
            return ((pathOld == "" || pathOld == "\\") && (pathNew == "" || pathNew == "\\"));
        }

        public void clear()
        {
            name = "";
            pathOld = "";
            pathNew = "";
            pathUploadFrom = "";
            URL = "";
        }

        public string asString()
        {
            return (name + ";" + pathOld + ";" + pathNew + ";" + pathUploadFrom + ";" + URL);
        }
    }
}
