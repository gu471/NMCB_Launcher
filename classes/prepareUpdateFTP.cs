using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using NMCB_Launcher.classes.structures;

namespace NMCB_Launcher.classes
{
    class prepareUpdateFTP
    {
        #region #VARS
        System.Windows.Forms.ListBox lb;
        public setConfig configs = new setConfig();
        public setForge forge = new setForge();
        public setMod mods = new setMod();
        public setOptional optional = new setOptional();
        public setRoot root = new setRoot();

        public int ftpVersion;
        #endregion

        public int getVersion(FTPworker ftp)
        {
            string ver = Directory.GetCurrentDirectory() + @"\" + ".version";
            ftp.Download(".version", ver, true);
            ftpVersion = Convert.ToInt16(File.ReadAllText(ver));
            File.Delete(ver);
            return ftpVersion;
        }
        public void getActualVersions(FTPworker ftp)
        {
            getVersion(ftp);
            forge.getVersion(ftp);
            optional.getVersion(ftp);
            configs.getVersion(ftp);
            mods.getVersion(ftp);
            root.getVersion(ftp);
        }        
        
        public prepareUpdateFTP(System.Windows.Forms.ListBox _lb)
        {
            lb = _lb;
        }
        public void uploadStruc(FTPworker ftp)
        {
            getActualVersions(ftp);

            bool updated = false;

            updated = configs.upload(ftp) || updated;
            updated = forge.upload(ftp) || updated;
            updated = optional.upload(ftp) || updated;
            updated = mods.upload(ftp) || updated;
            updated = root.upload(ftp) || updated;

            if (updated)
            {
                ftp.startUpload();
                string ver = Directory.GetCurrentDirectory() + @"\" + ".version";
                File.WriteAllText(ver, (ftpVersion + 1).ToString());
                ftp.Upload(ver, "/.version");
                File.Delete(ver);
            }
        }

        private List<string> getDrop(DragEventArgs e)
        {
            List<string> dropped = new List<string>();

            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                // e.Effect = DragDropEffects.Copy;
                dropped.Add(e.Data.GetData(DataFormats.Text).ToString());
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string drop in fileList)
                {
                    string cdrop = drop;

                    if (Directory.Exists(cdrop) && !(cdrop[cdrop.Length - 1] == '\\'))
                    {
                        cdrop += @"\";
                    }
                    dropped.Add(drop);
                }
            }
            return dropped;
        }

        #region ADD
        public void addConfigRec(string path, string folder)
        {
            string actualDir = "";

            if (Directory.Exists(path))
            {
                string[] dirs = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);


                string[] t = path.Replace(@"\\", @"\").Split('\\');
                actualDir = t[t.Length - 1];

                configs.addModOrConfig(path + @"\", folder + @"\" + actualDir + @"\");

                foreach (string dir in dirs)
                {
                    addConfigRec(dir, folder + @"\" + actualDir + @"\");
                }
                foreach (string file in files)
                {
                    addConfigRec(file, folder + @"\" + actualDir + @"\");
                }
            }
            else
                configs.addModOrConfig(path, folder + @"\");
        }
        public void addModRec(string path, string folder)
        {
            string actualDir = "";

            if (Directory.Exists(path))
            {
                string[] dirs = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);


                string[] t = path.Replace(@"\\", @"\").Split('\\');
                actualDir = t[t.Length - 1];

                mods.addModOrConfig(path + @"\", folder + @"\" + actualDir + @"\");

                foreach (string dir in dirs)
                {
                    addModRec(dir, folder + @"\" + actualDir + @"\");
                }
                foreach (string file in files)
                {
                    addModRec(file, folder + @"\" + actualDir + @"\");
                }
            }
            else
                mods.addModOrConfig(path, folder + @"\");
        }
        public void addRootRec(string path, string folder)
        {
            string actualDir = "";

            if (Directory.Exists(path))
            {
                string[] dirs = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);


                string[] t = path.Replace(@"\\", @"\").Split('\\');
                actualDir = t[t.Length - 1];

                root.addModOrConfig(path + @"\", folder + @"\" + actualDir + @"\");

                foreach (string dir in dirs)
                {
                    addRootRec(dir, folder + @"\" + actualDir + @"\");
                }
                foreach (string file in files)
                {
                    addRootRec(file, folder + @"\" + actualDir + @"\");
                }
            }
            else
                root.addModOrConfig(path, folder + @"\");
        }

        public void addConfig(DragEventArgs e, string folder = "")
        {
            foreach (string path in getDrop(e))
            {
                addConfigRec(path, folder);
            }
        }
        public void addMod(DragEventArgs e, string folder = "")
        {
            foreach (string path in getDrop(e))
            {
                addModRec(path, folder);
            }
        }
        public void addRoot(DragEventArgs e, string folder = "")
        {
            foreach (string path in getDrop(e))
            {
                addRootRec(path, folder);
            }
        }
        #endregion

        #region REPLACE
        public void replaceConfigRec(string pathOld, string drop)
        {

            if (Directory.Exists(drop))
            {
                string folder = Path.GetDirectoryName(pathOld);

                string[] dirs = drop.Split('\\');
                string actualDir = dirs[dirs.Length - 1];

                configs.replaceModOrConfig(folder + @"\" + Path.GetFileName(drop) + @"\", drop + @"\", "");

                foreach (string dir in Directory.GetDirectories(drop))
                {
                    replaceConfigRec(folder + @"\" + actualDir + @"\", dir);
                }

                foreach (string file in Directory.GetFiles(drop + @"\"))
                {
                    replaceConfigRec(folder + @"\" + actualDir + @"\", file);
                }
            }
            else
            {
                string folder = Path.GetDirectoryName(pathOld);

                configs.replaceModOrConfig(folder + @"\" + Path.GetFileName(drop), drop, "");
            }
        }

        public void replaceConfig(string pathOld, DragEventArgs e)
        {
            List<string> drops = getDrop(e);

            if (drops.Count == 1 && !Directory.Exists(drops.First()))
            {
                configs.replaceModOrConfig(pathOld, drops.First(), "");
            }
            else
                foreach (string drop in getDrop(e))
                {
                    replaceConfigRec(pathOld, drop);
                }
        }
        public void replaceForge(string pathOld, DragEventArgs e)
        {
            string path = getDrop(e).First();
            string folder = path.isURL() ? Path.GetDirectoryName(pathOld) : "";
            forge.replaceModOrConfig(pathOld, path, folder);
        }
        public void replaceMod(string pathOld, DragEventArgs e)
        {
            string path = getDrop(e).First();
            //string folder = path.isURL() ? Path.GetDirectoryName(pathOld) : "";
            string URL = path.isURL() ? path : "";
            mods.replaceModOrConfig(pathOld, path, URL);
        }
        public void replaceOptional(string pathOld, DragEventArgs e)
        {
            string path = getDrop(e).First();
            string URL = path.isURL() ? path : "";
            optional.replaceModOrConfig(pathOld, path, URL);
        }
        public void replaceRoot(string pathOld, DragEventArgs e)
        {
            string path = getDrop(e).First();
            //string folder = path.isURL() ? Path.GetDirectoryName(pathOld) : "";
            string URL = path.isURL() ? path : "";
            root.replaceModOrConfig(pathOld, path, URL);
        }
        #endregion

        #region DELETE
        public void deleteConfig(string pathToDelete)
        {
            configs.delete(pathToDelete);
        }
        public void deleteNeeded(string pathToDelete)
        {
            mods.delete(pathToDelete);
        }
        public void deleteRoot(string pathToDelete)
        {
            root.delete(pathToDelete);
        }                
        #endregion        
    }
}
