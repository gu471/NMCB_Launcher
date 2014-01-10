using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NMCB_Launcher.classes
{
    class profile
    {
        public string name { get; set; }
        public string gameDir { get; set; }
        public string lastVersionId { get; set; }
        public string javaArgs { get; set; }
    }

    public class SysUtil
    {
        public static String StringEncodingConvert(String strText, String strSrcEncoding, String strDestEncoding)
        {
            System.Text.Encoding srcEnc = System.Text.Encoding.GetEncoding(strSrcEncoding);
            System.Text.Encoding destEnc = System.Text.Encoding.GetEncoding(strDestEncoding);
            byte[] bData = srcEnc.GetBytes(strText);
            byte[] bResult = System.Text.Encoding.Convert(srcEnc, destEnc, bData);
            return destEnc.GetString(bResult);
        }
    }

    class profileWorker
    {
        public void init(string Version)
        {
            dynamic profileFile = getJsonProfile();

            if (!profileExists(profileFile))
            {
                profileFile = createProfile(profileFile, Version);
            }
            else
            {
                profileFile = onStart(profileFile, Version);
            }

            //WO MCPatcher
            profileFile.profiles.NMCBroz.lastVersionId = "NMCBroz";
            
            profileFile.selectedProfile = "NMCBroz";
            string json = JsonConvert.SerializeObject(profileFile, Formatting.Indented);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft\\launcher_profiles.json";
            File.WriteAllText(path, json, Encoding.Default);
        }

        private dynamic getJsonProfile()
        {
            string pathLocal = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\launcher_profiles.json";
            string json = File.ReadAllText(pathLocal);
            return JObject.Parse(json);
        }

        private bool profileExists(dynamic pf)
        {
            try
            {
                string pfName = pf.profiles.NMCBroz.name;
                return true;
            }
            catch //{ Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e; }
            {
                return false;
            }
        }    
 
        private dynamic createProfile(dynamic pf, string version)
        {
            profile NMCBroz = new profile();
            NMCBroz.name = "NMCBroz";
            NMCBroz.gameDir = SysUtil.StringEncodingConvert(Directory.GetCurrentDirectory() + @"\minecraft\", "ISO-8859-1", "UTF-8");
            NMCBroz.lastVersionId = version;

            ulong ram = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
            ram = (ulong)(ram / Math.Pow(1024, 2) / 1000);
            NMCBroz.javaArgs = "-Xmx" + (ram > 4 ? 4 : ram - 1) + "G -XX:PermSize\u003d512m -Dfml.ignorePatchDiscrepancies\u003dtrue -Dfml.ignoreInvalidMinecraftCertificates\u003dtrue";

            pf.profiles.Add("NMCBroz", JToken.FromObject(NMCBroz));

            return pf;
        }

        private dynamic onStart(dynamic pf, string version)
        {
            string gamedir = Directory.GetCurrentDirectory() + @"\minecraft\";
            
            if (pf.profiles.NMCBroz.gameDir != gamedir)
            {
                pf.profiles.NMCBroz.gameDir = SysUtil.StringEncodingConvert(gamedir, "ISO-8859-1", "UTF-8");
            }

            if (pf.profiles.NMCBroz.lastVersionId != version)
            {
                pf.profiles.NMCBroz.lastVersionId = version;
            }

            return pf;
        }
  
        public void debug(RichTextBox rtb)
        {
            string pathLocal = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\launcher_profiles.json";
            string json = File.ReadAllText(pathLocal);

            dynamic d = JObject.Parse(json);

            rtb.addLine((string) JsonConvert.SerializeObject(d, Formatting.Indented));
        }
    }
}
