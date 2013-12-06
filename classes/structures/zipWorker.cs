using System;
using System.IO;
using System.Windows.Forms;

using ICSharpCode.SharpZipLib.Zip;

namespace NMCB_Launcher.classes.structures
{
    class zipWorker
    {
        RichTextBox rtbDebug;

        public zipWorker(RichTextBox _rtb)
        {
            rtbDebug = _rtb;
        }

        public void Unzip(string args)
        {
            // Perform simple parameter checking.

            if (!File.Exists(args))
            {
                Console.WriteLine("Cannot find file '{0}'", args[0]);
                return;
            }

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(args)))
            {

                ZipEntry theEntry;
                rtbDebug.addLine("zipping...");
                while ((theEntry = s.GetNextEntry()) != null)
                {

                    Console.WriteLine(theEntry.Name);

                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    // create directory
                    if (directoryName.Length > 0)
                    {
                        if (!Directory.Exists(directoryName))
                            rtbDebug.addLine(" create " + directoryName);
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fileName != String.Empty)
                    {
                        rtbDebug.addLine(" uz_file: " + fileName);
                        using (FileStream streamWriter = File.Create(theEntry.Name))
                        {

                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}