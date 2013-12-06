using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

using NMCB_Launcher.classes.structures;

namespace NMCB_Launcher.classes
{
    class prepareUpdateLocal
    {
        #region #VARS
        public setConfig configs = new setConfig();
        public setForge forge = new setForge();
        public setMod mods = new setMod();
        public setOptional optional = new setOptional();
        public setRoot root = new setRoot();

        INIWorker localConfig = new INIWorker();

        public httpWorker http;
        NetworkCredential htaccess;
        #endregion

        public int remoteOverall;

        public prepareUpdateLocal()
        {
            securityWorker sec = new securityWorker();
            htaccess = sec.getAccess();
        }

        public void initVersions()
        {
            INIWorker ini = new INIWorker();
            configs.dataVersion = ini.getLocalConfig();
            forge.dataVersion = ini.getLocalForge();
            mods.dataVersion = ini.getLocalNeeded();
            optional.dataVersion = ini.getLocalOptional();
            root.dataVersion = ini.getLocalRoot();
        }

        public void initLiteLoader()
        {
            // \minecraft\liteconfig\common\VoxelMenu\VoxelServer.properties

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\minecraft\liteconfig\common\VoxelMenu\");
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\minecraft\liteconfig\common\VoxelMenu\VoxelServer.properties", "Text=NMCBroz\r\nIp=46.228.195.79:25565");
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\minecraft\liteconfig\common\voxelmenu.properties", "#Wed Dec 04 00:06:03 CET 2013\r\nupgraded=true\r\nshowTexturePacksOnMainMenu=false\r\nusePrettyIngameMenu=true\r\nserverIP=46.228.195.79\\:25565\r\n\r\nmenuVolume=.5\r\nmute=false\r\nusePrettyTexturePacksScreen=true\r\nserverTXT=NMCBroz");
        }

        public void prepareDownload(bool reinstallOptional = false, bool optionalOnly = false)
        {
            if (!optionalOnly)
            {
                root.http = http;
                root.prepareDownload();

                forge.http = http;
                forge.prepareDownload();

                configs.http = http;
                configs.prepareDownload();

                mods.http = http;
                mods.prepareDownload();
            }

            optional.http = http;
            optional.prepareDownload(reinstallOptional);
        }

        public int getNewRemoteOverall()
        {
            WebClient wc = new WebClient();
            wc.Credentials = htaccess;
            string remoteOverall = wc.DownloadString(localConfig.getModBase() + ".version");

            return Convert.ToInt16(remoteOverall);
        }

        public void checkRemoteUpdates(RichTextBox rtb, ProgressBar pb, bool reinstall = false)
        {
            localConfig = new INIWorker();

            int i;
            int j;
            int k;
            int l;
            int m;

            i = reinstall ? -1 : localConfig.getLocalConfig();
            j = reinstall ? -1 : localConfig.getLocalForge();
            k = reinstall ? -1 : localConfig.getLocalNeeded();
            l = reinstall ? -1 : localConfig.getLocalOptional();
            m = reinstall ? -1 : localConfig.getLocalRoot();

            int vOverall = reinstall ? -1 : localConfig.getLocalOverall();
            int vRemoteOverall = getNewRemoteOverall();
            remoteOverall = vRemoteOverall;

            rtb.addLine(" overall : " + vOverall.ToString() + " < " + vRemoteOverall);

            if (vRemoteOverall > vOverall)
            {
                pb.Value = 0;
                pb.Maximum = 5;
                if (forge.gotNewRemoteVersion(j))
                {
                    rtb.addLine("forge : updating");
                    forge.getRemoteVersion(j);
                }
                pb.Value++;

                if (configs.gotNewRemoteVersion(i))
                {
                    rtb.addLine("config : updating");
                    configs.getRemoteVersion(i);
                }
                pb.Value++;

                if (mods.gotNewRemoteVersion(k))
                {
                    rtb.addLine("needed : updating");
                    mods.getRemoteVersion(k);
                }
                pb.Value++;

                if (optional.gotNewRemoteVersion(l))
                {
                    rtb.addLine("optional : updating");
                    optional.getRemoteVersion(l);
                }
                pb.Value++;

                if (root.gotNewRemoteVersion(m))
                {
                    rtb.addLine("info : updating");
                    root.getRemoteVersion(m);
                }
                pb.Value++;

                rtb.addLine("  trim.");
                forge.trim();
                configs.trim();
                mods.trim();
                optional.trim();
                root.trim();
            }
        }
    }
}
