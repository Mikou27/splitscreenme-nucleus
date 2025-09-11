using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Nucleus.Gaming.App.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Nucleus.Gaming
{
    public static class Localization
    {
        private static string defaultLang = "en";//default
        private static string localizationFilePath;

        private static string localiazationsPath = Path.Combine(Globals.NucleusInstallRoot, "locals");
        private static string localiazationFileExtension = ".json";//.txt?
        private static Dictionary<int, string> localizationData = new Dictionary<int, string>();

        public static void LoadLocalization()//To call from App_Settings_Loader as last? 
        {
            if (Directory.Exists(localiazationsPath))
            {
                localizationFilePath = Path.Combine(localiazationsPath, $"{App_Misc.Language}{localiazationFileExtension}");

                if(!File.Exists(localizationFilePath))
                {
                    //localization file not found log something
                    //should not happen because we will list all the files from settings and fallback to en if the required file is not found anyway. 

                    localizationFilePath = Path.Combine(localiazationsPath, $"{defaultLang}{localiazationFileExtension}");
                }

                ReadLocalizationFile();
            }
        }

        public static string GetLocalizedText(int index)
        {
            return localizationData[index];
        }

        private static void ReadLocalizationFile()
        {

            string jsonString = File.ReadAllText(localizationFilePath);

            JObject jLocalFile = (JObject)JsonConvert.DeserializeObject(jsonString);

            for (int i = 0; i < jLocalFile.Count; i++)
            {
                localizationData.Add(i, (string)jLocalFile[i.ToString()]);
            }
        }

        public static void ConsoleListLocalization()
        {
            for (int i = 0; i < localizationData.Count; i++)
            {
                Console.WriteLine(localizationData[i]);
            }
        }
    }
}
