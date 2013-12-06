using System;
using System.Collections;
using System.IO;

namespace NMCB_Launcher.classes
{
    public class INIWorker
    {
        private Hashtable keyPairs = new Hashtable();
        private String iniFilePath;

        #region SET
        public void setOptionals(string optionals)
        {
            AddSetting("default", "optionals", optionals);
            SaveSettings();
        }

        public void setModBase(string modBase)
        {
            AddSetting("default", "modBase", modBase);
            SaveSettings();
        }

        public void setVersion(string version)
        {
            AddSetting("default", "version", version);
            SaveSettings();
        }

        #region FTP
        public void setFtpHost(string ftpHost)
        {
            AddSetting("admin", "ftpHost", ftpHost);
            SaveSettings();
        }
        public void setFtpUser(string ftpUser)
        {
            AddSetting("admin", "ftpUser", ftpUser);
            SaveSettings();
        }
        public void setFtpPass(string ftpPass)
        {
            AddSetting("admin", "ftpPass", ftpPass);
            SaveSettings();
        }
        #endregion
        #endregion

        #region VERSIONS
        public void setChocVersion(int version)
        {
            AddSetting("versions", "choc", version.ToString());
            SaveSettings();
        }
        public void setLocalConfig(int version)
        {
            AddSetting("versions", "config", version.ToString());
            SaveSettings();
        }   
        public void setLocalForge(int version)
        {
            AddSetting("versions", "forge", version.ToString());
            SaveSettings();
        }
        public void setLocalNeeded(int version)
        {
            AddSetting("versions", "needed", version.ToString());
            SaveSettings();
        }
        public void setLocalOptional(int version)
        {
            AddSetting("versions", "optional", version.ToString());
            SaveSettings();
        }
        public void setLocalRoot(int version)
        {
            AddSetting("versions", "root", version.ToString());
            SaveSettings();
        }
        public void setLocalOverall(int version)
        {
            AddSetting("versions", "overall", version.ToString());
            SaveSettings();
        }
        public void setTPVersion(int version)
        {
            AddSetting("versions", "tp", version.ToString());
            SaveSettings();
        }
        #endregion

        #region GET local version

        public string getModBase()
        {
            return GetSetting("default", "modBase");
        }
        public string getOptionals()
        {
            return GetSetting("default", "optionals");
        }
        //MCP
        public string getVersion()
        {
            return GetSetting("default", "version");
        }

        #region FTP
        public string getFtpHost()
        {
            return GetSetting("admin", "ftpHost");
        }
        public string getFtpUser()
        {
            return GetSetting("admin", "ftpUser");
        }
        public string getFtpPass()
        {
            return GetSetting("admin", "ftpPass");
        }
        #endregion

        #region VERSIONS
        public int getChocVersion()
        {
            return Convert.ToInt16(GetSetting("versions", "choc"));
        }
        public int getLocalConfig()
        {
            return Convert.ToInt16(GetSetting("versions", "config"));
        }
        public int getLocalForge()
        {
            return Convert.ToInt16(GetSetting("versions", "forge"));
        }
        public int getLocalNeeded()
        {
            return Convert.ToInt16(GetSetting("versions", "needed"));
        }
        public int getLocalOptional()
        {
            return Convert.ToInt16(GetSetting("versions", "optional"));
        }
        public int getLocalOverall()
        {
            return Convert.ToInt16(GetSetting("versions", "overall"));
        }
        public int getLocalRoot()
        {
            return Convert.ToInt16(GetSetting("versions", "root"));
        }
        public int getTPVersion()
        {
            return Convert.ToInt16(GetSetting("versions", "tp"));
        }
        #endregion

        #endregion

        #region INIWorker
        //template found at:	http://stackoverflow.com/questions/217902/reading-writing-an-ini-file
        //referencing:		    http://bytes.com/topic/net/insights/797169-reading-parsing-ini-file-c
        private struct SectionPair
        {
            public String Section;
            public String Key;
        }

        /// <summary>
        /// Opens the INI file at the given path and enumerates the values in the IniParser.
        /// </summary>
        /// <param name="iniPath">Full path to INI file.</param>
        public INIWorker()
        {
            TextReader iniFile = null;
            String strLine = null;
            String currentRoot = null;
            String[] keyPair = null;            

            iniFilePath = Directory.GetCurrentDirectory() + @"\config\settings.ini";

            if (File.Exists(iniFilePath))
            {
                try
                {
                    iniFile = new StreamReader(iniFilePath);

                    strLine = iniFile.ReadLine();

                    while (strLine != null)
                    {
                        strLine = strLine.Trim(); //.ToUpper();

                        if (strLine != "")
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            else
                            {
                                keyPair = strLine.Split(new char[] { '=' }, 2);

                                SectionPair sectionPair;
                                String value = null;

                                if (currentRoot == null)
                                    currentRoot = "ROOT";

                                sectionPair.Section = currentRoot;
                                sectionPair.Key = keyPair[0];

                                if (keyPair.Length > 1)
                                    value = keyPair[1];

                                keyPairs.Add(sectionPair, value);
                            }
                        }

                        strLine = iniFile.ReadLine();
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                }
            }
            else
                throw new FileNotFoundException("Unable to locate " + iniFilePath);

        }

        /// <summary>
        /// Returns the value for the given section, key pair.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="settingName">Key name.</param>
        private String GetSetting(String sectionName, String settingName)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName;//.ToUpper();
            sectionPair.Key = settingName;//.ToUpper();

            return (String)keyPairs[sectionPair];
        }

        /// <summary>
        /// Enumerates all lines for given section.
        /// </summary>
        /// <param name="sectionName">Section to enum.</param>
        private String[] EnumSection(String sectionName)
        {
            ArrayList tmpArray = new ArrayList();

            foreach (SectionPair pair in keyPairs.Keys)
            {
                if (pair.Section == sectionName)//.ToUpper())
                    tmpArray.Add(pair.Key);
            }

            return (String[])tmpArray.ToArray(typeof(String));
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        /// <param name="settingValue">Value of key.</param>
        private void AddSetting(String sectionName, String settingName, String settingValue)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName;//.ToUpper();
            sectionPair.Key = settingName;//.ToUpper();

            if (keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);

            keyPairs.Add(sectionPair, settingValue);
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved with a null value.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        private void AddSetting(String sectionName, String settingName)
        {
            AddSetting(sectionName, settingName, null);
        }

        /// <summary>
        /// Remove a setting.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        private void DeleteSetting(String sectionName, String settingName)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName;//.ToUpper();
            sectionPair.Key = settingName;//.ToUpper();

            if (keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);
        }

        /// <summary>
        /// Save settings to new file.
        /// </summary>
        /// <param name="newFilePath">New file path.</param>
        private void SaveSettings(String newFilePath)
        {
            ArrayList sections = new ArrayList();
            String tmpValue = "";
            String strToSave = "";

            foreach (SectionPair sectionPair in keyPairs.Keys)
            {
                if (!sections.Contains(sectionPair.Section))
                    sections.Add(sectionPair.Section);
            }

            foreach (String section in sections)
            {
                strToSave += ("[" + section + "]\r\n");

                foreach (SectionPair sectionPair in keyPairs.Keys)
                {
                    if (sectionPair.Section == section)
                    {
                        tmpValue = (String)keyPairs[sectionPair];

                        if (tmpValue != null)
                            tmpValue = "=" + tmpValue;

                        strToSave += (sectionPair.Key + tmpValue + "\r\n");
                    }
                }

                strToSave += "\r\n";
            }

            try
            {
                TextWriter tw = new StreamWriter(newFilePath);
                tw.Write(strToSave);
                tw.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Save settings back to ini file.
        /// </summary>
        private void SaveSettings()
        {
            SaveSettings(iniFilePath);
        }
        #endregion
    }        
}
