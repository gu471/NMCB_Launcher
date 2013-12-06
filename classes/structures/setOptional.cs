using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using GlacialComponents.Controls;

namespace NMCB_Launcher.classes.structures
{
    class setOptional : updateSet
    {
        private string ftpRoot = "/optional/";

        public List<string> possible = new List<string>();
        public List<string> wanted = new List<string>();
        public List<string> unWanted = new List<string>();
        public List<string> fromConfig = new List<string>();

        public List<string> reconfigAdd = new List<string>();
        public List<string> reconfigRemove = new List<string>();

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
        
        #region FileHandling
        public void prepareDownload(bool reinstall = false) // reinstall == update for onUpdate
        {
            string pathLocal = Directory.GetCurrentDirectory() + @"\minecraft\mods\";
            if (!Directory.Exists(pathLocal))
            {
                Directory.CreateDirectory(pathLocal);
                Debug.WriteLine("creating " + pathLocal);
            }

            trim();
            string[] delLocal = Directory.GetFiles(pathLocal);

            if (reinstall)
            {
                // lösche unwanted
                // installier wanted
                // |=> data = data - unwanted
                // |=> del unwanted local

                foreach (updateItem hold in data)
                {
                    foreach (string del in unWanted)
                    {
                        if (hold.pathNew.Contains(del) || hold.pathOld.Contains(del))
                            hold.clear();
                    }
                }

                foreach (string file in delLocal)
                {
                    foreach (string del in unWanted)
                    {
                        if (file.Contains(del))
                            File.Delete(file);
                    }
                }
            }
            else
            {
                // lösche remove
                // installier add
                // |=> data = data if in add
                // |=> del remove local

                foreach (updateItem hold in data)
                {
                    bool clearIt = true;

                    foreach (string add in reconfigAdd)
                    {
                        if (hold.pathNew.Contains(add) || hold.pathOld.Contains(add))
                            clearIt = false;
                    }

                    if (clearIt) hold.clear();
                }

                foreach (string file in delLocal)
                {
                    foreach (string del in reconfigRemove)
                    {
                        if (file.Contains(del))
                            File.Delete(file);
                    }
                }
            }

            trim();

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
        #endregion

        #region SerialInterface
        public List<string> stringToWanted(string str)
        {

            List<string> wanted = new List<string>();

            var list = str.Split(';').Select(tag => tag.Trim()).Where(tag => !string.IsNullOrEmpty(tag));

            foreach (string s in list)
            {
                wanted.Add(s);
            }

            return wanted;
        }

        public string wantedToString(List<string> wanted)
        {
            string back = "";

            foreach (string wants in wanted)
            {
                back += wants + ";";
            }

            return back;
        }
        #endregion

        #region Outbound
        private string getDescription(string name)
        {
            switch (name)
            {
                case "ArmorStatusHUD":
                    return "Zeigt deine angelegte Ausrüstung an sowie deren Haltbarkeit.";
                case "DirectionHUD":
                    return "Zeigt dir einen Kompass im HUD an.";
                case "StatusEffectHUD":
                    return "Zeigt dir im HUD die aktuell auf dich wirkenden Effekte an.";
                case "DynamicLights":
                    return "Fakeln in der Hand und es ist immernoch dunkel?\r\nJetzt nicht mehr ;)";
                case "mod_macros":
                    return "Erlaubt das erstellen von Makros, die direkt über \r\neine Tastenkombination ausgeführt werden können.";
                case "BackTools":
                    return "Du wolltest schon immer ein Schwert auf dem Rücken tragen?\r\nJetzt kannst du es.";
                case "ChatBubbles":
                    return "Mit diesem Mod kannst du schneller erkennen, welcher Spieler\r\nin deiner Umgebung gerade was geschrieben hat.";
                case "Mouse Tweaks":
                    return "Mit diesem Mod kannst du das Crafting und dein Inventar noch\r\nbesser händeln.";
                case "neiaddons":
                    return "Wie war das doch gleich mit den Bienen?";
                case "NEIPlugins":
                    return "Zeigt dir mehr Rezepte an, als das StandardNEI.";
                case "Waila":
                    return "Was für einen Block sehe ich gerade an?\r\n(Zeigt auch den dazugehörigen Mod an)";
                case "mapwriter":
                    return "Ein Map Mod, mit dem du eine Karte der von\r\ndir erkundeten Welt anschauen kannst.";
                case "ZansMinimap":
                    return "Eine Minimap mit Radar und Höhlenmodus.";
                case "DamageIndicators":
                    return "Wieviel HP hat der Mob denn noch, bis er stirbt?";
                case "TabbyChat":
                    return "Eine Alternative zu Chat Bubbles.";
                case "hdskins":
                    return "Erlaubt das Benutzen von hochauflösenden Skins.";
                case "VoxelCam":
                    return "Ein Screenshotmanager, mit dem du unter anderem\r\ndirekt auf Twitter, Dropbox und Imgur hochladen kannst.";
                case "voxelplayer":
                    return "Bessere Namensplatten über Spielern.";
                case "voxeltextures":
                    return "Texturepack on-the-fly wechseln.";
                case "voxelvision":
                    return "Ermöglicht Anpassung der Sichteinstellungen.";
                case "WMLL":
                    return "Zeigt dir den aktuellen Lichtlevel an und vieles mehr!";
                default:
                    return "";
            }
        }
        private string getIsRecommended(string name)
        {
            switch (name)
            {
                case "Waila":
                case "neiaddons":
                case "NEIPlugins":
                case "Mouse Tweaks":
                case "ArmorStatusHUD":
                case "DirectionHUD":
                case "voxelvision":
                case "voxeltextures":
                case "ZansMinimap":
                case "mod_macros":
                case "DamageIndicators":
                    return "ja";
                default:
                    return "";
            }
        }
        private GLItem getOptionalListItem(string name, bool isInstalled)
        {
            GLItem gli = new GLItem();
            gli.SubItems[1].Text = name;
            gli.SubItems[1].Checked = isInstalled;
            gli.SubItems[2].Text = getDescription(name);
            gli.SubItems[0].Text = getIsRecommended(name);
            return gli;
        }
        private void setListView(GlacialList gl)
        {
            gl.Items.Clear();
            gl.Columns.Clear();

            GLColumn glc;
            glc = new GLColumn("empf.");
            glc.Width = 40;
            glc.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            gl.Columns.Add(glc);
            glc = new GLColumn("Name");
            glc.Width = 120;
            glc.CheckBoxes = true;
            gl.Columns.Add(glc);
            gl.Columns.Add("Beschreibung", 340);

            possible.Sort();

            foreach (string opt in possible)
            {
                if (fromConfig.FindIndex(x => x.Contains(opt)) < 0)
                {
                    gl.Items.Add(getOptionalListItem(opt, false));
                }
                else
                {
                    ListViewItem lvi = new ListViewItem(opt);
                    lvi.Checked = true;
                    gl.Items.Add(getOptionalListItem(opt, true));
                }
            }

            gl.SortColumn(0);
        }

        public void setFromConfig(GlacialList gl)
        {
            INIWorker ini = new INIWorker();

            fromConfig = stringToWanted(ini.getOptionals());

            setPossibles();
            setListView(gl);
            setWanted(gl);
        }
        public void setWanted(GlacialList gl)
        {
            wanted.Clear();
            /*
            foreach (ListViewItem item in lv.Items)
            {
                if (item.Checked)
                    wanted.Add(item.Text);
            }*/

            foreach (GLItem gli in gl.Items)
            {
                if (gli.SubItems[1].Checked)
                {
                    wanted.Add(gli.SubItems[1].Text);
                }
            }

            setUnWanted();

            checkBspkrsCore();
        }

        public void setToConfig()
        {
            INIWorker ini = new INIWorker();

            ini.setOptionals(wantedToString(wanted));
        }
        #endregion
        
        #region SET

        public void setReconfigAdd()
        {
            reconfigAdd.Clear();

            foreach (string s in wanted)
            {
                if (fromConfig.FindIndex(x => x.Contains(s)) < 0)
                {
                    reconfigAdd.Add(s);
                }
            }
        }
        public void setReconfigRemove()
        {
            reconfigRemove.Clear();

            foreach (string s in fromConfig)
            {
                if (wanted.FindIndex(x => x.Contains(s)) < 0)
                {
                    reconfigRemove.Add(s);
                }
            }
        }        
        private void setUnWanted()
        {
            unWanted.Clear();

            foreach (string pos in possible)
            {              
                if (wanted.FindIndex(x => x.Contains(pos)) < 0)
                {
                    unWanted.Add(pos);
                }
            }
        }
        public void setPossibles()
        {
            possible.Clear();

            if (File.Exists(@"config\optional.list"))
            {
                string[] optionals = File.ReadAllLines(@"config\optional.list");
                foreach (string opt in optionals)
                {
                    possible.Add(opt);
                }
            }

        }

        public void checkBspkrsCore()
        {
            int bspk = wanted.FindIndex(x => x.Contains("bspkrsCore"));
            if (bspk > -1)
            {
                wanted.RemoveAt(bspk);
            }

            foreach (string wants in wanted)
            {
                if (wants.Contains("ArmorStatusHUD") || wants.Contains("DirectionHUD") || wants.Contains("StatusEffectHUD"))
                {
                    wanted.Add("bspkrsCore");
                    return;
                }
            }

            unWanted.Add("bspkrsCore");
        }
        #endregion
    }
}
