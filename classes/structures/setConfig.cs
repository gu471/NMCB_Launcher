using System.Diagnostics;
using System.IO;

namespace NMCB_Launcher.classes.structures
{
    class setConfig : updateSet
    {
        private string ftpRoot = "/config/";

        public void prepareDownload()
        {
            string pathLocal = Directory.GetCurrentDirectory() + @"\minecraft\config\";
            if (!Directory.Exists(pathLocal))
            {
                Directory.CreateDirectory(pathLocal);
                Debug.WriteLine("creating " + pathLocal);
            }

            _download(ftpRoot, pathLocal);
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
