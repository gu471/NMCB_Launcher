using System.Diagnostics;
using System.IO;

namespace NMCB_Launcher.classes.structures
{
    class pathCheck
    {
        public string pathUL = "";
        public string path = "";

        public pathCheck(string _path)
        {
            if (_path.isURL())
            {
                string fileName = _path.getFile();

                if (!fileName.Contains("."))
                {
                    httpWorker http = new httpWorker(null, null, null, null);
                    fileName = http.getFileNameFormHeader(_path);
                    fileName = fileName.Split(';')[0].Replace("\"" , "");
                }

                Debug.WriteLine(fileName);

                path = fileName;
            }
            else
            {
                pathUL = _path;
                path = Path.GetFileName(_path);
            }
        }
    }
}
