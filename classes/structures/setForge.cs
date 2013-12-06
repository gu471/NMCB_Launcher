using System;
using System.Diagnostics;
using System.IO;

namespace NMCB_Launcher.classes.structures
{
    class setForge : updateSet
    {
        private string ftpRoot = "/forge/";

        public void prepareDownload()
        {
            string pathLocal = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\";
            if (!Directory.Exists(pathLocal))
            {
                Directory.CreateDirectory(pathLocal);
                Debug.WriteLine("creating " + pathLocal);
            }

            _download(ftpRoot, pathLocal); 
        }

        public string getFileByName(FTPworker ftp, string subString)
        {
            FTPdirectory list = ftp.ListDirectoryDetail(ftpRoot);
            FTPdirectory files = list.GetFiles();

            foreach (FTPfileInfo file in files)
            {
                if (file.FullName.Contains(subString))
                    return file.FullName.Replace(ftpRoot, "/");
            }

            return "";
        }

        public bool upload(FTPworker ftp)
        {
            return _upload(ftp, ftpRoot);
        }

        public int getVersion(FTPworker ftp)
        {
            return _getVersion(ftp, ftpRoot);
        }

        public bool gotNewRemoteVersion(int localVersion)
        {
            return _gotNewRemoteVersion(localVersion, ftpRoot);
        }

        public void getRemoteVersion(int version)
        {
            _getRemoteVersion(version, ftpRoot);
        }
    }
}
