
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using System.Collections.Concurrent;
using System.Collections.Generic;

using NMCB_Launcher.classes;
using NMCB_Launcher.classes.structures;
using updateSystemDotNet.Core.Types;

namespace NMCB_Launcher
{


    public partial class Form1 : Form
    {
        #region VARS
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(
              IntPtr hWnd,      // handle to destination window
              uint Msg,       // message
              long wParam,  // first message parameter
              long lParam   // second message parameter
              );

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        INIWorker settings = new INIWorker();
        prepareUpdateFTP adminUpdate;
        prepareUpdateLocal localUpdate;
        prepareUpdateLocal Changelog;

        NetworkCredential htaccess;

        string startArgs = "";
        bool isInstall = false;
        string keyBinds = "key_key.attack:-100\r\nkey_key.use:-99\r\nkey_key.forward:17\r\nkey_key.left:30\r\nkey_key.back:31\r\nkey_key.right:32\r\nkey_key.jump:57\r\nkey_key.sneak:42\r\nkey_key.drop:74\r\nkey_key.inventory:18\r\nkey_key.chat:20\r\nkey_key.playerlist:15\r\nkey_key.pickItem:-98\r\nkey_key.command:53\r\nkey_VoxelMods:66\r\nkey_key.macro_override:0\r\nkey_Freeze Time:0\r\nkey_Normal Time:0\r\nkey_Time Offset +:0\r\nkey_Time Offset -:0\r\nkey_ScreenShot Manager:35\r\nkey_Big Screenshot:62\r\nkey_key.minimap.voxelmapmenu:50\r\nkey_key.tarmor:24\r\nkey_key.tcapes.reload:88\r\nkey_key.minimap.voxelmapmenu:50\r\nkey_IC2 ALT Key:56\r\nkey_IC2 Boost Key:29\r\nkey_IC2 Mode Switch Key:51\r\nkey_IC2 Side Inventory Key:0\r\nkey_IC2 Hub Expand Key:0\r\nkey_Gravi Fly Key:16\r\nkey_Clipboard:0\r\nkey_Ender Pack:0\r\nkey_Force Belt:0\r\nkey_Force Key:83\r\nkey_Force Belt Slot 1:79\r\nkey_Force Belt Slot 2:80\r\nkey_Force Belt Slot 3:81\r\nkey_Force Belt Slot 4:75\r\nkey_Force Belt Slot 5:76\r\nkey_Force Belt Slot 6:77\r\nkey_Force Belt Slot 7:71\r\nkey_Force Belt Slot 8:72\r\nkey_Reposition Mob Portrait:68\r\nkey_Change Wand Focus:33\r\nkey_Dynamic Lights toggle:13\r\nkey_waila.keybind.wailaconfig:82\r\nkey_waila.keybind.wailadisplay:0\r\nkey_waila.keybind.liquid:0\r\nkey_waila.keybind.recipe:0\r\nkey_waila.keybind.usage:0\r\nkey_key.mw_open_gui:49\r\nkey_key.mw_new_marker:0\r\nkey_key.mw_next_map_mode:0\r\nkey_key.mw_next_marker_group:0\r\nkey_key.mw_teleport:0\r\nkey_key.mw_zoom_in:0\r\nkey_key.mw_zoom_out:0\r\nkey_Mekanism Mode Switch:52\r\nkey_Mekanism Voice:0\r\nkey_key.craftingGrid:0\r\nkey_xact.clear:208\r\nkey_xact.load:0\r\nkey_xact.prev:203\r\nkey_xact.next:205\r\nkey_xact.delete:211\r\nkey_xact.openGrid:46\r\nkey_keybind.loco.faster:0\r\nkey_keybind.loco.slower:0\r\nkey_keybind.loco.mode:0\r\nkey_keybind.loco.whistle:0\r\nkey_key.control:0\r\nkey_FullBright:0\r\nkey_Clear Weather:0\r\nkey_key.macros:58";
        #endregion

        #region DLLs
        private Dictionary<string, string> assemblyNameToFileMapping = new Dictionary<string, string>();
        private void GetAssemblyNames()
        {
            string folderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lib");
            foreach (string file in Directory.EnumerateFiles(folderPath, "*.dll"))
            {
                try
                {
                    if (file.Contains("GlacialList1.3.dll"))
                    {
                        AssemblyName name = AssemblyName.GetAssemblyName(file);
                        assemblyNameToFileMapping.Add(name.FullName, file);
                    }
                }
                catch { } // Just move on if we can't get the name.
            }
        }
        private Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            string file;
            if (assemblyNameToFileMapping.TryGetValue(args.Name, out file))
            {
                return Assembly.LoadFrom(file);
            }
            return null;
        }
        #endregion

        #region INIT
        public Form1(string[] args)
        {
            GetAssemblyNames();
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);

            InitializeComponent();
            adminUpdate = new prepareUpdateFTP(lbAdminLog);
            localUpdate = new prepareUpdateLocal();
            Changelog = new prepareUpdateLocal();

            System.Text.StringBuilder sbArgs;

            sbArgs = new System.Text.StringBuilder("Übergebene Parameter:");
            foreach (string arg in args)
            {
                startArgs += arg + " ";
            }

            securityWorker sec = new securityWorker();
            htaccess = sec.getAccess();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("restart"))
            {
                startArgs = File.ReadAllText("restart");
                File.Delete("restart");
            }

            this.MouseWheel += new MouseEventHandler(optionalOnMouseWheel);

            lAdminForgeSelected.Text = "";
            lAdminConfigSelected.Text = "";
            lAdminNeededSelected.Text = "";
            lAdminOptionalSelected.Text = "";
            lAdminRootSelected.Text = "";
            updAdminNeededDelete.Enabled = false;
            updAdminConfigDelete.Enabled = false;
            updAdminRootDelete.Enabled = false;
            glInstallOptional.Items.Clear();

            if (!Directory.Exists("Info"))
                Directory.CreateDirectory("Info");

            tabControlAdmin.TabPages.Remove(tabAdminFTP);
            tabControlAdmin.TabPages.Remove(tabAdminChangelog);
            tabControlAdmin.TabPages.Add(tabAdminFTP);
            tabControlAdmin.TabPages.Add(tabAdminChangelog);

            localUpdate.optional.setFromConfig(glInstallOptional);
            setMCPRadio();

            setChangeConfigButton();

            tabDebug.Text = updateController1.Version;

            if (!startArgs.Contains("-debug"))
            {
                tabControl.TabPages.Remove(tabUpdate);
                tabControl.TabPages.Remove(tabInstall);
                tabControl.TabPages.Remove(tabAdmin);
                tabControl.TabPages.Remove(tabDebug);
                bUpdateCheck.Visible = false;
                bUpdateStartMC.Visible = false;
                bInstallStartMC.Visible = false;
                dummy.Visible = false;
                panel1.Visible = false;
                bChangelogMergeConfig.Visible = false;
                bChangelogMergeForge.Visible = false;
                bChangelogMergeMods.Visible = false;
                bChangelogMergeOptional.Visible = false;
                bChangelogMergeRoot.Visible = false;
            }

            if (!startArgs.Contains("-debug2") && updateController1.checkForUpdates())
            {
                updateController1.checkForUpdatesAsync();
                tabControl.TabPages.Add(tabUpdate);
                isInstall = true;
                File.WriteAllText("restart", startArgs);
            }
            else
            {
                if (!File.Exists(@"lib\Minecraft.exe"))
                {
                    MessageBox.Show("Minecraft.exe wurde nicht gefunden und wird jetzt heruntergeladen");
                    WebClient wc = new WebClient();
                    wc.DownloadFile("https://s3.amazonaws.com/Minecraft.Download/launcher/Minecraft.exe", @"lib\Minecraft.exe");
                }

                INIWorker settings = new INIWorker();

                cbInstallTP.Checked = settings.getTPVersion() == -99 ? true : false;

                if (!startArgs.Contains("-debug"))
                {
                    if (settings.getLocalOverall() < 0)
                    {
                        this.Text = "NMCBroz Installation (C) gu471";
                        tabControl.TabPages.Add(tabInstall);
                        cbLockInstall.Visible = false;
                        isInstall = true;
                        tabInstall.Text = "Installieren";
                    }
                    else if (startArgs.Contains("-config"))
                    {
                        this.Text = "NMCBroz Konfiguration (C) gu471";
                        tabControl.TabPages.Add(tabInstall);
                        cbLockInstall.Checked = cbLockInstall.Visible;
                        bInstall.Enabled = !cbLockInstall.Visible;
                        tabInstall.Text = "Konfigurieren";
                    }
                    else if (startArgs == "")
                    {
                        this.Text = "NMCBroz Launcher (C) gu471";
                        tabControl.TabPages.Add(tabUpdate);
                    }

                    if (startArgs.Contains("-admin"))
                    {
                        tabControl.TabPages.Add(tabAdmin);
                        this.Text = "NMCBroz Adminpanel (C) gu471";
                    }
                }
                else
                {
                    this.Text = "NMCBroz DebugSession (C) gu471";
                    cbLockInstall.Checked = true;
                    bInstall.Enabled = false;
                }
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            if (startArgs == "" && !isInstall)
            {
                updateMC();
                startMC();
            }
        }
        #endregion
        
        #region Configurate Install/Reconfig
        private void setMCPRadio()
        {
            settings = new INIWorker();
            if (settings.getVersion() == "NMCBrozMCP")
            {
                rbMCPtrue.Select();
            }
            else
            {
                rbMCPfalse.Select();
            }
        }
        private void setChangeConfigButton()
        {
            INIWorker settings = new INIWorker();

            bInstallChange.Enabled = (settings.getLocalOverall() > -1) ? true : false;
            bInstallChange.Text = (bInstallChange.Enabled) ? "Konfiguration ändern" : "(brozMC ist nicht installiert)";
        }
        public void setKeyBinds()
        {
            string optionsPath = Directory.GetCurrentDirectory() + @"\minecraft\options.txt";

            if (!File.Exists(optionsPath))
            {
                File.WriteAllText(optionsPath, keyBinds);
            }
            else
            {
                string[] lines = File.ReadAllLines(optionsPath);
                string newFile = "";

                for (int i = 0; i <= lines.Length - 1; i++)
                {
                    if (!lines[i].Contains("key_"))
                    {
                        newFile += lines[i] + "\r\n";
                    }
                }

                newFile += keyBinds;
                File.WriteAllText(optionsPath, newFile);
            }
        }

        private void optionalOnMouseWheel(object sender, MouseEventArgs e)
        {
            if (tabControl.SelectedTab != tabInstall)
                return;

            int pos;
            try
            {
                pos = (int)glInstallOptional.SelectedIndicies[0];
            }
            catch
            {
                pos = 0;
            }
            int posBar = glInstallOptional.vPanelScrollBar.Value;
            int diff = (e.Delta != 0) ? (e.Delta > 0) ? -1 : 1 : 0;
            int maxBar = glInstallOptional.vPanelScrollBar.Maximum;
            int count = glInstallOptional.Count;

            int newpos = pos + diff;
            int newposBar = posBar + diff;
            newpos = (newpos >= 0) ? (newpos > count - 1) ? count - 1 : newpos : 0;
            newposBar = (newposBar >= 0) ? (newposBar > maxBar) ? maxBar - 1 : newposBar : 0;

            glInstallOptional.Items[newpos].Selected = true;
            glInstallOptional.vPanelScrollBar.Value = newposBar;
            glInstallOptional.FocusedItem = glInstallOptional.Items[newpos];
        }
        private void cleanRecursive()
        {
            string path = Directory.GetCurrentDirectory() + @"\minecraft\mods";
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                if (!dir.Contains("VoxelMods") && !dir.Contains("macros") && !dir.Contains("WMLL"))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        private void cbLockInstall_CheckedChanged(object sender, EventArgs e)
        {
            bInstall.Enabled = !cbLockInstall.Checked;
        }
        private void bInstall_Click(object sender, EventArgs e)
        {
            //dl Minecraft.exe

            Directory.CreateDirectory(@"minecraft");
            Directory.CreateDirectory(@"minecraft\config");
            Directory.CreateDirectory(@"minecraft\mods");

            if (cbInstallClean.Checked)
            {
                setKeyBinds();
                Directory.Delete(Directory.GetCurrentDirectory() + @"\minecraft\mods", true);
                Directory.Delete(Directory.GetCurrentDirectory() + @"\minecraft\config", true);
            }
            else
            {
                cleanRecursive();
                Directory.Delete(Directory.GetCurrentDirectory() + @"\minecraft\config", true);
            }

            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\launcher_profiles.json"))
            {
                Process.Start(Directory.GetCurrentDirectory() + @"\lib\Minecraft.exe");

                bool notInit = true;
                while (notInit)
                {
                    Process[] processlist = Process.GetProcesses();

                    foreach (Process process in processlist)
                    {
                        if (process.MainWindowTitle.Contains("Minecraft Launcher 1."))
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            SendKeys.SendWait("%{F4}");
                            notInit = false;
                        }
                    }
                }
            }

            localUpdate.optional.setWanted(glInstallOptional);

            lInstallActual.Text = "initialisiere Download";
            localUpdate.optional.setToConfig();
            settings.setVersion(rbMCPtrue.Checked ? "NMCBrozMCP" : "NMCBroz");
            Application.DoEvents();
            rtbDebug.addLine("== Install ==");
            httpWorker http = new httpWorker(pbInstall, pbInstallOverall, lInstallActual, rtbDebug);

            localUpdate.checkRemoteUpdates(rtbDebug, pbInstall, true);

            localUpdate.http = http;
            localUpdate.prepareDownload(true);
            //localUpdate.initLiteLoader();
            bool newChoc = prepareChoc(http, true);
            prepareTexturePack(http, true);            

            if (http.Count() < 1)
                lInstallActual.Text = "fertig.";

            http.startDownload();

            while (lInstallActual.Text != "fertig.")
            {
                Application.DoEvents();
            }

            if (newChoc)
            {
                lInstallActual.Text = "unzipping Chocolate";
                Application.DoEvents();
                unzipChocolate();
            }
            lInstallActual.Text = "unzipping resources";
            Application.DoEvents();
            unzipResources();
            unzipTP();
            lInstallActual.Text = "fertig.";

            INIWorker ini = new INIWorker();
            ini.setLocalConfig(localUpdate.configs.dataVersion);
            ini.setLocalForge(localUpdate.forge.dataVersion);
            ini.setLocalNeeded(localUpdate.mods.dataVersion);
            ini.setLocalOptional(localUpdate.optional.dataVersion);
            ini.setLocalOverall(localUpdate.remoteOverall);

            setChangeConfigButton();

            if (isInstall || cbInstallClean.Checked)
                setKeyBinds();

            if (!startArgs.Contains("-debug"))
            {
                DialogResult dialogResult = MessageBox.Show("Minecraft wurde installiert.\r\n\r\n Jetzt Starten?", "Minecraft starten.", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Yes)
                {
                    startMC();
                }
                else if (dialogResult == DialogResult.No)
                {
                    Application.Exit();
                }
            }
        }
        private void bInstallReConfig_Click(object sender, EventArgs e)
        {
            localUpdate.optional.setWanted(glInstallOptional);
            localUpdate.optional.setReconfigAdd();
            localUpdate.optional.setReconfigRemove();

            settings = new INIWorker();
            settings.setVersion(rbMCPtrue.Checked ? "NMCBrozMCP" : "NMCBroz");

            lInstallActual.Text = "initialisiere Download";
            Application.DoEvents();
            rtbDebug.addLine("== change config ==");
            httpWorker http = new httpWorker(pbInstall, pbInstallOverall, lInstallActual, rtbDebug);
            
            localUpdate.optional.getRemoteVersion(-1);

            localUpdate.http = http;
            localUpdate.prepareDownload(false, true);
            prepareTexturePack(http);

            if (http.Count() == 0)
                lInstallActual.Text = "fertig.";

            http.startDownload();

            while (lInstallActual.Text != "fertig.")
            {
                Application.DoEvents();
            }

            unzipTP();

            localUpdate.optional.setToConfig();
            localUpdate.optional.setFromConfig(glInstallOptional);

            setKeyBinds();

            if (!startArgs.Contains("-debug"))
            {
                DialogResult dialogResult = MessageBox.Show("Konfiguration wurde gespeichert.\r\n\r\n Jetzt Starten?", "Minecraft starten.", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Yes)
                {
                    startMC();
                }
                else if (dialogResult == DialogResult.No)
                {
                    Application.Exit();
                }
            }
        }
        private void bInstallKeyBind_Click(object sender, EventArgs e)
        {
            setKeyBinds();
        }
        #endregion

        #region prepareData
        private bool prepareChoc(httpWorker http, bool reinstall = false)
        {
            INIWorker settings = new INIWorker();
            int localVersion = settings.getChocVersion();
            WebClient wc = new WebClient();
            wc.Credentials = htaccess;
            int remoteVersion = Convert.ToInt16(wc.DownloadString(settings.getModBase() + "stuff/.chocversion"));

            if ((remoteVersion > localVersion) || reinstall)
            {
                rtbDebug.addLine("choc: " + localVersion + "<" + remoteVersion);
                if (Directory.Exists(@"minecraft\Chocolate\"))
                    Directory.Delete(@"minecraft\Chocolate\", true);
                //Directory.CreateDirectory(@"minecraft\resourcepacks\");
                string dest = Directory.GetCurrentDirectory() + @"\minecraft\Chocolate.zip";
                string source = settings.getModBase() + "stuff/Chocolate.zip";
                http.addToDownload(new downloadItem(source, dest));
                settings.setChocVersion(remoteVersion);
                return true;
            }

            return false;
        }
        private void prepareTexturePack(httpWorker http, bool reinstall = false)
        {
            INIWorker settings = new INIWorker();

            if (cbInstallTP.Checked)
            {
                if (File.Exists(@"minecraft\resourcepacks\brozPack.zip"))
                    File.Delete(@"minecraft\resourcepacks\brozPack.zip");
                settings.setTPVersion(-99);
            }
            else
            {
                int localVersion = settings.getTPVersion();
                WebClient wc = new WebClient();
                wc.Credentials = htaccess;
                int remoteVersion = Convert.ToInt16(wc.DownloadString(settings.getModBase() + "stuff/.tpversion"));

                if ((remoteVersion > localVersion) || reinstall)
                {
                    rtbDebug.addLine("tp: " + localVersion + "<" + remoteVersion);
                    Directory.CreateDirectory(@"minecraft\resourcepacks\");
                    string dest = Directory.GetCurrentDirectory() + @"\minecraft\resourcepacks\brozPack.zip";
                    string dest2 = Directory.GetCurrentDirectory() + @"\minecraft\resourcepacks\brozPack.7z";
                    if (File.Exists(dest))
                    {
                        FileInfo filePath = new FileInfo(dest);
                        FileAttributes attribute;
                        attribute = (FileAttributes)(FileAttributes.Normal);

                        File.SetAttributes(filePath.FullName, attribute);
                        File.Delete(dest);
                    }
                    string source = settings.getModBase() + "stuff/brozPack.7z";
                    http.addToDownload(new downloadItem(source + ".001", dest2 + ".001"));
                    http.addToDownload(new downloadItem(source + ".002", dest2 + ".002"));
                    http.addToDownload(new downloadItem(source + ".003", dest2 + ".003"));
                    settings.setTPVersion(remoteVersion);
                }

                string optionsPath = Directory.GetCurrentDirectory() + @"\minecraft\options.txt";

                if (!File.Exists(optionsPath))
                    File.WriteAllText(optionsPath, "skin:brozPack.zip");
                else
                {
                    string[] lines = File.ReadAllLines(optionsPath);
                    bool tpInstalled = false;

                    for (int i = 0; i <= lines.Length - 1; i++)
                    {
                        if (lines[i].Contains("skin:brozPack"))
                        {
                            return;
                        }
                        else if (lines[i].Contains("skin:"))
                        {
                            lines[i] = "skin:brozPack.zip";
                            tpInstalled = true;
                            break;
                        }
                    }

                    if (!tpInstalled)
                    {
                        Array.Resize(ref lines, lines.Length + 1);
                        lines[lines.Length - 1] = "skin:brozPack.zip";
                    }

                    File.WriteAllLines(optionsPath, lines);
                }
            }
        }
        public void unzipChocolate()
        {
            BlockingCollection<string> StdOut = new BlockingCollection<string>();
            string exePath = Directory.GetCurrentDirectory();
            string chocpath = exePath + @"\minecraft\Chocolate\";
            if (Directory.Exists(chocpath))
                Directory.Delete(chocpath, true);
            Directory.CreateDirectory(chocpath);
            Directory.SetCurrentDirectory(exePath + @"\minecraft\");
            Process zip = new Process();
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = exePath + @"\lib\7zip\7z.exe";
            processStartInfo.Arguments = "x \"" + exePath + @"\minecraft\Chocolate.zip" + "\"";
            rtbDebug.addLine(processStartInfo.FileName + " " + processStartInfo.Arguments);
            zip.StartInfo = processStartInfo;
            if (startArgs.Contains("-debug"))
            {
                zip.StartInfo.UseShellExecute = false;
                zip.StartInfo.RedirectStandardOutput = true;


                zip.OutputDataReceived += new DataReceivedEventHandler(
                    (s, e) =>
                    {
                        StdOut.TryAdd(e.Data);
                    }
                );
            }
            zip.Start();

            if (startArgs.Contains("-debug"))
            {
                zip.BeginOutputReadLine();

                while (!zip.HasExited)
                {
                    string output;
                    StdOut.TryTake(out output);
                    if (output != null)
                        rtbDebug.addLine(output);
                    Application.DoEvents();
                }
            }
            else
            {
                zip.WaitForExit();
                File.Delete("Chocolate.zip");
            }            
            Directory.SetCurrentDirectory(exePath);
        }

        public void unzipTP()
        {
            BlockingCollection<string> StdOut = new BlockingCollection<string>();
            string exePath = Directory.GetCurrentDirectory();
            string rpath = exePath + @"\minecraft\resourcepacks\";
            if (File.Exists(rpath + "BrozPack.7z.001"))
            {
                if (File.Exists(rpath + @"brozPack.zip"))
                    File.Delete(rpath + @"brozPack.zip");
                Directory.SetCurrentDirectory(rpath);
                Process zip = new Process();
                var processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = exePath + @"\lib\7zip\7z.exe";
                processStartInfo.Arguments = "x \"" + rpath + "brozPack.7z.001\"";
                rtbDebug.addLine(processStartInfo.FileName + " " + processStartInfo.Arguments);
                zip.StartInfo = processStartInfo;
                if (startArgs.Contains("-debug"))
                {
                    zip.StartInfo.UseShellExecute = false;
                    zip.StartInfo.RedirectStandardOutput = true;


                    zip.OutputDataReceived += new DataReceivedEventHandler(
                        (s, e) =>
                        {
                            StdOut.TryAdd(e.Data);
                        }
                    );
                }
                zip.Start();

                if (startArgs.Contains("-debug"))
                {
                    zip.BeginOutputReadLine();

                    while (!zip.HasExited)
                    {
                        string output;
                        StdOut.TryTake(out output);
                        if (output != null)
                            rtbDebug.addLine(output);
                        Application.DoEvents();
                    }
                }
                else
                {
                    zip.WaitForExit();
                    File.Delete(rpath + "BrozPack.7z.001");
                    File.Delete(rpath + "BrozPack.7z.002");
                    File.Delete(rpath + "BrozPack.7z.003");
                }
                Directory.SetCurrentDirectory(exePath);
            }
        }

        public void unzipResources()
        {
            BlockingCollection<string> StdOut = new BlockingCollection<string>();
            string exePath = Directory.GetCurrentDirectory();
            string modpath = exePath + @"\minecraft\mods\";
            if (File.Exists(modpath + "resources.rar"))
            {
                if (Directory.Exists(modpath + @"resources\"))
                    Directory.Delete(modpath + @"resources\", true);
                Directory.SetCurrentDirectory(modpath);
                Process zip = new Process();
                var processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = exePath + @"\lib\7zip\7z.exe";
                processStartInfo.Arguments = "x \"" + modpath + "resources.rar\"";
                rtbDebug.addLine(processStartInfo.FileName + " " + processStartInfo.Arguments);
                zip.StartInfo = processStartInfo;
                if (startArgs.Contains("-debug"))
                {
                    zip.StartInfo.UseShellExecute = false;
                    zip.StartInfo.RedirectStandardOutput = true;


                    zip.OutputDataReceived += new DataReceivedEventHandler(
                        (s, e) =>
                        {
                            StdOut.TryAdd(e.Data);
                        }
                    );
                }
                zip.Start();

                if (startArgs.Contains("-debug"))
                {
                    zip.BeginOutputReadLine();

                    while (!zip.HasExited)
                    {
                        string output;
                        StdOut.TryTake(out output);
                        if (output != null)
                            rtbDebug.addLine(output);
                        Application.DoEvents();
                    }
                }
                else
                {
                    zip.WaitForExit();
                    File.Delete(modpath + "resources.rar");
                }
                Directory.SetCurrentDirectory(exePath);
            }
        }
        #endregion

        #region Upload
        private void bUpload_Click(object sender, EventArgs e)
        {
            lbAdminLog.Items.Clear();
            lbAdminLog.Items.Add("initializing upload, please wait");
            Application.DoEvents();

            FTPworker ftp = new FTPworker();
            ftp.Hostname = settings.getFtpHost();
            ftp.Username = settings.getFtpUser();
            ftp.Password = settings.getFtpPass();

            if (!Directory.Exists("ul"))
                Directory.CreateDirectory("ul");

            //button3_Click(sender, e);
            ftp.barFile = pbAdminUpload;
            ftp.barOverall = pbAdminUploadOverall;
            ftp.lb = lbAdminLog;
            adminUpdate.uploadStruc(ftp);

            if (Directory.Exists("ul"))
                Directory.Delete("ul", true);

            lbAdminLog.Items.Add("upload completed");
            this.Text = "NMCBroz Adminpanel (C) gu471";
            lbAdminConfigLog.Items.Clear();
            lbAdminForgeLog.Items.Clear();
            lbAdminModsLog.Items.Clear();
            lbAdminRootLog.Items.Clear();
            lbAdminOptionalLog.Items.Clear();
        }
        private void bCheckSet_Click(object sender, EventArgs e)
        {
            lbAdminLog.Items.Clear();

            foreach (updateItem mod in adminUpdate.mods.data)
            {
                lbAdminLog.Items.Add(mod.name + ";" + mod.pathOld + ";" + mod.pathNew + ";" + mod.pathUploadFrom + ";" + mod.URL);
            }

            foreach (updateItem config in adminUpdate.configs.data)
            {
                lbAdminLog.Items.Add(config.name + ";" + config.pathOld + ";" + config.pathNew + ";" + config.pathUploadFrom + ";" + config.URL);
            }
        }
        private void bgetFtpVer_Click(object sender, EventArgs e)
        {
            FTPworker ftp = new FTPworker();
            ftp.Hostname = settings.getFtpHost();
            ftp.Username = settings.getFtpUser();
            ftp.Password = settings.getFtpPass();

            adminUpdate.getActualVersions(ftp);

            lFtpVer.Text = adminUpdate.ftpVersion.ToString();

            lFtpVerForge.Text = adminUpdate.forge.ftpVersion.ToString();
            lFtpVerOptional.Text = adminUpdate.optional.ftpVersion.ToString();
            lFtpVerConfig.Text = adminUpdate.configs.ftpVersion.ToString();
            lFtpVerNeeded.Text = adminUpdate.mods.ftpVersion.ToString();
            lFtpVerMaps.Text = adminUpdate.root.ftpVersion.ToString();
        }
        #endregion

        #region AdminPanel
        #region DragNDrop
        private void onDragEnter(object sender, DragEventArgs e)
        {
            onDragEnter(sender, e, false);
        }
        private void onDragEnter(object sender, DragEventArgs e, bool onlyOne = false, bool allowURL = true)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop) && onlyOne)
                {
                    string[] list = (string[])e.Data.GetData(DataFormats.FileDrop);
                    e.Effect = list.Length == 1 ? DragDropEffects.Copy : DragDropEffects.None;

                    if (Directory.Exists(list[0])) e.Effect = DragDropEffects.None;
                }
                else
                    e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effect = (e.Data.GetData(DataFormats.Text).ToString().isURL() && allowURL) ? DragDropEffects.Copy : DragDropEffects.None;
            }
            else
                e.Effect = DragDropEffects.None;

        }

        private void updAdminConfigAdd_DragDrop(object sender, DragEventArgs e)
        {
            adminUpdate.addConfig(e, getFolder(tvAdminConfig));
            logParse(lbAdminConfigLog, adminUpdate.configs.data);
        }
        private void updAdminModAdd_DragDrop(object sender, DragEventArgs e)
        {

            adminUpdate.addMod(e, getFolder(tvAdminNeeded));
            logParse(lbAdminModsLog, adminUpdate.mods.data);

        }
        private void updAdminRootAdd_DragDrop(object sender, DragEventArgs e)
        {
            adminUpdate.addRoot(e, getFolder(tvAdminRoot));
            logParse(lbAdminRootLog, adminUpdate.root.data);
        }

        private void updAdminConfigAdd_DragEnter(object sender, DragEventArgs e)
        {
            onDragEnter(sender, e, false, false);
        }
        
        private void updAdminConfigReplace_DragDrop(object sender, DragEventArgs e)
        {
            adminUpdate.replaceConfig(lAdminConfigSelected.Text, e);
            logParse(lbAdminConfigLog, adminUpdate.configs.data);
        }
        private void updAdminForgeReplace_DragDrop(object sender, DragEventArgs e)
        {
            adminUpdate.replaceForge(lAdminForgeSelected.Text, e);
            logParse(lbAdminForgeLog, adminUpdate.forge.data);
        }
        private void updAdminNeededReplace_DragDrop(object sender, DragEventArgs e)
        {
            adminUpdate.replaceMod(lAdminNeededSelected.Text, e);
            logParse(lbAdminModsLog, adminUpdate.mods.data);
        }
        private void updAdminOptionalReplace_DragDrop(object sender, DragEventArgs e)
        {
            adminUpdate.replaceOptional(lAdminOptionalSelected.Text, e);
            logParse(lbAdminOptionalLog, adminUpdate.optional.data);
        }
        private void updAdminRootReplace_DragDrop(object sender, DragEventArgs e)
        {
            adminUpdate.replaceRoot(lAdminRootSelected.Text, e);
            logParse(lbAdminRootLog, adminUpdate.root.data);
        }

        private void updAdminConfigReplace_DragEnter(object sender, DragEventArgs e)
        {
            if (lAdminConfigSelected.Text != "")
            {
                onDragEnter(sender, e, false, false);
            }
        }
        private void updAdminForgeReplace_DragEnter(object sender, DragEventArgs e)
        {
            onDragEnter(sender, e, true, false);
        }
        private void updAdminNeededReplace_DragEnter(object sender, DragEventArgs e)
        {
            if (lAdminNeededSelected.Text != "" && lAdminNeededSelected.Text[lAdminNeededSelected.Text.Length - 1] != '\\')
            {
                onDragEnter(sender, e, true);
            }
        }
        private void updAdminOptionalReplace_DragEnter(object sender, DragEventArgs e)
        {
            onDragEnter(sender, e, true, true);
        }
        private void updAdminRootReplace_DragEnter(object sender, DragEventArgs e)
        {
            if (lAdminRootSelected.Text != "" && lAdminRootSelected.Text[lAdminRootSelected.Text.Length - 1] != '\\')
            {
                onDragEnter(sender, e, true);
            }
        }
        #endregion

        #region Delete
        private void updAdminConfigDelete_Click(object sender, EventArgs e)
        {
            adminUpdate.deleteConfig(lAdminConfigSelected.Text);
            logParse(lbAdminConfigLog, adminUpdate.configs.data);
        }
        private void updAdminNeededDelete_Click(object sender, EventArgs e)
        {
            adminUpdate.deleteNeeded(lAdminNeededSelected.Text);
            logParse(lbAdminModsLog, adminUpdate.mods.data);
        }
        private void updAdminRootDelete_Click(object sender, EventArgs e)
        {
            adminUpdate.deleteRoot(lAdminRootSelected.Text);
            logParse(lbAdminRootLog, adminUpdate.root.data);
        }
        #endregion

        #region Treeview
        private string getFolder(TreeView tv)
        {
            string folder = tv.SelectedNode != null ? tv.SelectedNode.FullPath.Replace(@"\\", @"\") : "";
            // Dir-Path
            folder = folder != "" ? Path.GetDirectoryName(folder) : "";
            folder = folder + @"\";
            return folder;
        }
        private void tvAdminConfig_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            lAdminConfigSelected.Text = e.Node.FullPath.Replace(@"\\", @"\");
            updAdminConfigDelete.Enabled = true;
        }
        private void tvAdminForge_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            lAdminForgeSelected.Text = e.Node.FullPath.Replace(@"\\", @"\");
        }
        private void tvAdminNeeded_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            lAdminNeededSelected.Text = e.Node.FullPath.Replace(@"\\", @"\");
            updAdminNeededDelete.Enabled = true;
        }
        private void tvAdminOptional_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            lAdminOptionalSelected.Text = e.Node.FullPath.Replace(@"\\", @"\");
        }
        private void tvAdminRoot_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            lAdminRootSelected.Text = e.Node.FullPath.Replace(@"\\", @"\");
            updAdminRootDelete.Enabled = true;
        }

        private void bAdminGetConfig_Click(object sender, EventArgs e)
        {
            tvAdminConfig.Nodes.Clear();
            lbAdminConfigLog.Items.Clear();
            lbAdminConfigLog.Items.Add("getting FTP struct, my take a while");
            Application.DoEvents();

            FTPworker ftp = new FTPworker();
            ftp.Hostname = settings.getFtpHost();
            ftp.Username = settings.getFtpUser();
            ftp.Password = settings.getFtpPass();

            ftp.directoryToTree("/config/", tvAdminConfig);

            lbAdminConfigLog.Items.Clear();
            logParse(lbAdminConfigLog, adminUpdate.configs.data);
        }        
        private void bAdminGetForge_Click(object sender, EventArgs e)
        {
            tvAdminForge.Nodes.Clear();
            lbAdminForgeLog.Items.Clear();
            lbAdminForgeLog.Items.Add("getting FTP struct, may take a while");
            Application.DoEvents();

            FTPworker ftp = new FTPworker();
            ftp.Hostname = settings.getFtpHost();
            ftp.Username = settings.getFtpUser();
            ftp.Password = settings.getFtpPass();

            ftp.directoryToTree("/forge/", tvAdminForge);

            lbAdminForgeLog.Items.Clear();
            logParse(lbAdminForgeLog, adminUpdate.forge.data);
        }
        private void bAdminGetNeeded_Click(object sender, EventArgs e)
        {
            tvAdminNeeded.Nodes.Clear();
            lbAdminModsLog.Items.Clear();
            lbAdminModsLog.Items.Add("getting FTP struct, my take a while");
            Application.DoEvents();

            FTPworker ftp = new FTPworker();
            ftp.Hostname = settings.getFtpHost();
            ftp.Username = settings.getFtpUser();
            ftp.Password = settings.getFtpPass();

            FTPdirectory list = ftp.ListDirectoryDetail("/needed/");
            ftp.directoryToTree("/needed/", tvAdminNeeded);

            lbAdminModsLog.Items.Clear();
            logParse(lbAdminModsLog, adminUpdate.mods.data);
        }
        private void bAdminGetOptional_Click(object sender, EventArgs e)
        {

            tvAdminOptional.Nodes.Clear();
            lbAdminOptionalLog.Items.Clear();
            lbAdminOptionalLog.Items.Add("getting FTP struct, my take a while");
            Application.DoEvents();

            FTPworker ftp = new FTPworker();
            ftp.Hostname = settings.getFtpHost();
            ftp.Username = settings.getFtpUser();
            ftp.Password = settings.getFtpPass();

            ftp.directoryToTree("/optional/", tvAdminOptional);

            lbAdminOptionalLog.Items.Clear();
            logParse(lbAdminOptionalLog, adminUpdate.optional.data);
        }
        private void bAdminGetRoot_Click(object sender, EventArgs e)
        {
            tvAdminRoot.Nodes.Clear();
            lbAdminRootLog.Items.Clear();
            lbAdminRootLog.Items.Add("getting FTP struct, my take a while");
            Application.DoEvents();

            FTPworker ftp = new FTPworker();
            ftp.Hostname = settings.getFtpHost();
            ftp.Username = settings.getFtpUser();
            ftp.Password = settings.getFtpPass();

            FTPdirectory list = ftp.ListDirectoryDetail("/root/");
            ftp.directoryToTree("/root/", tvAdminRoot);

            lbAdminRootLog.Items.Clear();
            logParse(lbAdminRootLog, adminUpdate.root.data);
        }
        #endregion

        #region Upload undo operation
        private void logParseFTP()
        {
            ListBox lb = lbAdminLog;

            lb.Items.Clear();
            if (adminUpdate.forge.data.Count > 0)
            {
                lb.Items.Add("forge:");
                logParse(lb, adminUpdate.forge.data, false, " ");
            }
            if (adminUpdate.mods.data.Count > 0)
            {
                lb.Items.Add("mods (needed):");
                logParse(lb, adminUpdate.mods.data, false, " ");
            }
            if (adminUpdate.optional.data.Count > 0)
            {
                lb.Items.Add("mods (optional):");
                logParse(lb, adminUpdate.optional.data, false, " ");
            }
            if (adminUpdate.configs.data.Count > 0)
            {
                lb.Items.Add("config:");
                logParse(lb, adminUpdate.configs.data, false, " ");
            }
            if (adminUpdate.root.data.Count > 0)
            {
                lb.Items.Add("root:");
                logParse(lb, adminUpdate.root.data, false, " ");
            }

            if (lb.Items.Count > 0)
                this.Text = "!!!! upload pending !!!!";
        }
        private void logParse(ListBox lb, List<updateItem> data, bool clear = true, string preString = "")
        {
            if (clear)
                lb.Items.Clear();
            for (int i = 0; i <= data.Count - 1; i++)
            {
                lb.Items.Add(preString + data[i].asString());
            }

            if (preString == "")
                logParseFTP();
        }

        private void logDeleteSelected(ListBox lb, List<updateItem> data, object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                int actPos = lb.SelectedIndex;
                if (actPos > -1)
                {
                    data.RemoveAt(actPos);

                    logParse(lb, data);

                    lb.SelectedIndex = (lb.Items.Count - 1 < actPos) ? actPos - 1 : actPos;
                }
            }
        }
        private void lbAdminConfigLog_KeyUp(object sender, KeyEventArgs e)
        {
            logDeleteSelected(lbAdminConfigLog, adminUpdate.configs.data, sender, e);
        }
        private void lbAdminForgeLog_KeyUp(object sender, KeyEventArgs e)
        {
            logDeleteSelected(lbAdminForgeLog, adminUpdate.forge.data, sender, e);
        }
        private void lbAdminModsLog_KeyUp(object sender, KeyEventArgs e)
        {
            logDeleteSelected(lbAdminModsLog, adminUpdate.mods.data, sender, e);
        }       
        private void lbAdminOptionalLog_KeyUp(object sender, KeyEventArgs e)
        {
            logDeleteSelected(lbAdminOptionalLog, adminUpdate.optional.data, sender, e);
        }
        private void lbAdminRootLog_KeyUp(object sender, KeyEventArgs e)
        {
            logDeleteSelected(lbAdminRootLog, adminUpdate.root.data, sender, e);
        }
        #endregion

        #region Changlelog for DebugStructure
        private void bChangelogConfigGet_Click(object sender, EventArgs e)
        {
            lbChangelogConfig.Items.Clear();
            lbChangelogConfig.Items.Add("getting changelog");

            if (cbChangelogConfigInit.Checked)
            {
                rtbDebug.addLine("== Changelog Config == reinstall");
                Changelog.configs.getRemoteVersion(-1);
                Changelog.configs.trim();
            }
            else
            {
                rtbDebug.addLine("== Changelog Config == fromVer: " + Convert.ToInt16(numChangelogConfig.Value));
                Changelog.configs.getRemoteVersion(Convert.ToInt16(numChangelogConfig.Value));
                Changelog.configs.trim();
            }

            lbChangelogConfig.Items.Clear();
            foreach (updateItem item in Changelog.configs.data)
            {
                if (item.pathOld == "\\")
                {
                    lbChangelogConfig.Items.Add("new: " + item.pathNew);
                }
                else if (item.pathNew == "\\")
                {
                    lbChangelogConfig.Items.Add("del: " + item.pathOld);
                }
                else
                {
                    lbChangelogConfig.Items.Add("change: " + item.pathOld + " => " + item.pathNew);
                }
            }
        }
        private void bChangelogForgeGet_Click(object sender, EventArgs e)
        {
            lbChangelogForge.Items.Clear();
            lbChangelogForge.Items.Add("getting changelog");

            if (cbChangelogForgeInit.Checked)
            {
                rtbDebug.addLine("== Changelog Forge == reinstall");
                Changelog.forge.getRemoteVersion(-1);
                Changelog.forge.trim();
            }
            else
            {
                rtbDebug.addLine("== Changelog Forge == fromVer: " + Convert.ToInt16(numChangelogForge.Value));
                Changelog.forge.getRemoteVersion(Convert.ToInt16(numChangelogForge.Value));
                Changelog.forge.trim();
            }

            lbChangelogForge.Items.Clear();
            foreach (updateItem item in Changelog.forge.data)
            {
                string fromURL = "";
                if (item.URL.isURL())
                {
                    fromURL = " || fromURL: " + item.URL;
                }
                if (item.pathOld == "\\")
                {
                    lbChangelogForge.Items.Add("new: " + item.pathNew + fromURL);
                }
                else if (item.pathNew == "\\")
                {
                    lbChangelogForge.Items.Add("del: " + item.pathOld + fromURL);
                }
                else
                {
                    lbChangelogForge.Items.Add("change: " + item.pathOld + " => " + item.pathNew + fromURL);
                }
            }
        }
        private void bChangelogModsGet_Click(object sender, EventArgs e)
        {
            lbChangelogMods.Items.Clear();
            lbChangelogMods.Items.Add("getting changelog");

            if (cbChangelogModsInit.Checked)
            {
                rtbDebug.addLine("== Changelog Needed == reinstall");
                Changelog.mods.getRemoteVersion(-1);
                Changelog.mods.trim();
            }
            else
            {
                rtbDebug.addLine("== Changelog Needed == fromVer: " + Convert.ToInt16(numChangelogMods.Value));
                Changelog.mods.getRemoteVersion(Convert.ToInt16(numChangelogMods.Value));
                Changelog.mods.trim();
            }

            lbChangelogMods.Items.Clear();
            foreach (updateItem item in Changelog.mods.data)
            {
                string fromURL = "";
                if (item.URL.isURL())
                {
                    fromURL = " || fromURL: " + item.URL;
                }
                if (item.pathOld == "\\")
                {
                    lbChangelogMods.Items.Add("new: " + item.pathNew + fromURL);
                }
                else if (item.pathNew == "\\")
                {
                    lbChangelogMods.Items.Add("del: " + item.pathOld + fromURL);
                }
                else
                {
                    lbChangelogMods.Items.Add("change: " + item.pathOld + " => " + item.pathNew + fromURL);
                }
            }
        }
        private void bChangelogOptionalGet_Click(object sender, EventArgs e)
        {
            lbChangelogOptional.Items.Clear();
            lbChangelogOptional.Items.Add("getting changelog");

            if (cbChangelogOptionalInit.Checked)
            {
                rtbDebug.addLine("== Changelog Optional == reinstall");
                Changelog.optional.getRemoteVersion(-1);
                Changelog.optional.trim();
            }
            else
            {
                rtbDebug.addLine("== Changelog Optional == fromVer: " + Convert.ToInt16(numChangelogOptional.Value));
                Changelog.optional.getRemoteVersion(Convert.ToInt16(numChangelogOptional.Value));
                Changelog.optional.trim();
            }

            lbChangelogOptional.Items.Clear();
            foreach (updateItem item in Changelog.optional.data)
            {
                string fromURL = "";
                if (item.URL.isURL())
                {
                    fromURL = " || fromURL: " + item.URL;
                }
                if (item.pathOld == "\\")
                {
                    lbChangelogOptional.Items.Add("new: " + item.pathNew + fromURL);
                }
                else if (item.pathNew == "\\")
                {
                    lbChangelogOptional.Items.Add("del: " + item.pathOld + fromURL);
                }
                else
                {
                    lbChangelogOptional.Items.Add("change: " + item.pathOld + " => " + item.pathNew + fromURL);
                }
            }
        }        
        private void bChangelogRootGet_Click(object sender, EventArgs e)
        {
            lbChangelogRoot.Items.Clear();
            lbChangelogRoot.Items.Add("getting changelog");

            if (cbChangelogRootInit.Checked)
            {
                rtbDebug.addLine("== Changelog Config == reinstall");
                Changelog.root.getRemoteVersion(-1);
                Changelog.root.trim();
            }
            else
            {
                rtbDebug.addLine("== Changelog Config == fromVer: " + Convert.ToInt16(numChangelogConfig.Value));
                Changelog.root.getRemoteVersion(Convert.ToInt16(numChangelogConfig.Value));
                Changelog.root.trim();
            }

            lbChangelogRoot.Items.Clear();
            foreach (updateItem item in Changelog.root.data)
            {
                if (item.pathOld == "\\")
                {
                    lbChangelogRoot.Items.Add("new: " + item.pathNew);
                }
                else if (item.pathNew == "\\")
                {
                    lbChangelogRoot.Items.Add("del: " + item.pathOld);
                }
                else
                {
                    lbChangelogRoot.Items.Add("change: " + item.pathOld + " => " + item.pathNew);
                }
            }
        }
        #endregion
        #endregion

        #region UpdateController
        private void updateController1_checkForUpdatesCompleted(object sender, updateSystemDotNet.appEventArgs.checkForUpdatesCompletedEventArgs e)
        {
            updateController1.checkForUpdatesCompleted -= updateController1_checkForUpdatesCompleted;
            if (e.Error != null)
            {
                throw e.Error;
            }
        }

        private void updateController1_downloadUpdatesCompleted(object sender, AsyncCompletedEventArgs e)
        {
            updateController1.downloadUpdatesCompleted -= updateController1_downloadUpdatesCompleted;
            updateController1.downloadUpdatesProgressChanged -= updateController1_downloadUpdatesProgressChanged;

            //Überprüfen ob das Update durch einen Fehler oder den Benutzer abgebrochen wurde
            if (e.Error != null)
                throw e.Error;

            //Download war erfolgreich, dann den Updateprozess starten
            //ini.AddSetting("custom", "version", updateController1.releaseInfo.Version);
            //ini.SaveSettings();

            updateController1.applyUpdate();
        }

        private void updateController1_downloadUpdatesProgressChanged(object sender, updateSystemDotNet.appEventArgs.downloadUpdatesProgressChangedEventArgs e)
        {
            pbUpdateDownload.Value = e.ProgressPercentage;
        }

        private void updateController1_updateFound(object sender, updateSystemDotNet.appEventArgs.updateFoundEventArgs e)
        {
            //if (!startArgs.Contains("-debug2"))
            Console.WriteLine("found2");
            {

                if (startArgs.Contains("-config") || startArgs.Contains("-admin"))
                {
                    tabControl.TabPages.Add(tabUpdate);
                    tabControl.SelectedTab = tabUpdate;
                }
                tabControl.Enabled = false;
                MessageBox.Show("Eine neue Version des Launchers wird heruntergeladen und gleich installiert");

                var sbChangelog = new StringBuilder();

                for (int i = e.Result.newUpdatePackages.Count - 1; i >= 0; i--)
                {
                    updatePackage package = e.Result.newUpdatePackages[i];
                    sbChangelog.AppendLine(e.Result.Changelogs[package].englishChanges);
                }

                string latestVersion = e.Result.newUpdatePackages[e.Result.newUpdatePackages.Count - 1].releaseInfo.Version;

                //Aktualisiere Launcher !!!

                updateController1.releaseInfo.Version = latestVersion;
                updateController1.downloadUpdatesCompleted += updateController1_downloadUpdatesCompleted;
                updateController1.downloadUpdatesProgressChanged += updateController1_downloadUpdatesProgressChanged;
                updateController1.downloadUpdates();
            }
        }
        #endregion

        #region start and update
        private void startMC()
        {
            profileWorker pw = new profileWorker();
            INIWorker settings = new INIWorker();
            pw.init(settings.getVersion());

            Process.Start(Directory.GetCurrentDirectory() + @"\lib\Minecraft.exe");

            Application.Exit();
        }
        private void bStartMC_Click(object sender, EventArgs e)
        {
            startMC();
        }

        private void updateMC()
        {
            localUpdate.optional.setWanted(glInstallOptional);
            localUpdate.initVersions();

            lUpdateActual.Text = "initialisiere Download";
            Application.DoEvents();
            rtbDebug.addLine("== Update ==");
            httpWorker http = new httpWorker(pbUpdate, pbUpdateOverall, lUpdateActual, rtbDebug);

            localUpdate.checkRemoteUpdates(rtbDebug, pbUpdate);

            localUpdate.http = http;
            localUpdate.prepareDownload(true);
            bool newChoc = prepareChoc(http);
            prepareTexturePack(http);

            if (http.Count() < 1)
                lUpdateActual.Text = "fertig.";

            http.startDownload();

            while (lUpdateActual.Text != "fertig.")
            {
                Application.DoEvents();
            }

            if (newChoc)
            {
                lUpdateActual.Text = "unzipping Chocolate";
                Application.DoEvents();
                unzipChocolate();
            }
            lUpdateActual.Text = "unzipping resources";
            Application.DoEvents();
            unzipResources();
            unzipTP();

            INIWorker ini = new INIWorker();
            ini.setLocalConfig(localUpdate.configs.dataVersion);
            ini = new INIWorker();
            ini.setLocalForge(localUpdate.forge.dataVersion);
            ini.setLocalNeeded(localUpdate.mods.dataVersion);
            ini.setLocalOptional(localUpdate.optional.dataVersion);
            ini.setLocalRoot(localUpdate.root.dataVersion);
            ini.setLocalOverall(localUpdate.getNewRemoteOverall());
        }
        private void bUpdate_Click(object sender, EventArgs e)
        {
            updateMC();
        }

        #endregion

        private void bDebugProfile_Click(object sender, EventArgs e)
        {
            profileWorker pw = new profileWorker();

            pw.debug(rtbDebug);
        }

        private void dummy_Click(object sender, EventArgs e)
        {
            unzipResources();
        }

        private void bChangelogMergeForge_Click(object sender, EventArgs e)
        {
            Changelog.forge.writeFile("_forge");
        }

        private void bChangelogMergeMods_Click(object sender, EventArgs e)
        {
            Changelog.mods.writeFile("_mods");
        }

        private void bChangelogMergeOptional_Click(object sender, EventArgs e)
        {
            Changelog.optional.writeFile("_optional");
        }

        private void bChangelogMergeConfig_Click(object sender, EventArgs e)
        {
            Changelog.configs.writeFile("_config");
        }

        private void bChangelogMergeRoot_Click(object sender, EventArgs e)
        {
            Changelog.root.writeFile("_root");
        }

        private void bConvert_Click(object sender, EventArgs e)
        {
            string temp = "\n" + rtbDebug.Text;
            rtbDebug.Clear();

            temp = temp.Replace("\t","").Replace("|", "\n").Replace("\n\n", "\n").Replace("\n\n", "\n");
            temp = temp.Replace("twd", "towny.wild.destroy").Replace("twb", "towny.wild.build").Replace("twu", "towny.wild.item_use").Replace("tws", "towny.wild.switch");
            temp = temp.Replace("tcad", "towny.claimed.alltown.destroy").Replace("tcab", "towny.claimed.alltown.build").Replace("tcau", "towny.claimed.alltown.item_use").Replace("tcas", "towny.claimed.alltown.switch");
            temp = temp.Replace("tcod", "towny.claimed.owntown.destroy").Replace("tcob", "towny.claimed.owntown.build").Replace("tcou", "towny.claimed.owntown.item_use").Replace("tcos", "towny.claimed.owntown.switch");

            temp = temp.Replace("\n", "\n    - ");

            rtbDebug.Text = temp;
        }
    }
}
