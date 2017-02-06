using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImportRenewals.Helpers
{
    public class Settings
    {
        private static string GetValue(string chave)
        {
            System.Configuration.AppSettingsReader appReader = new System.Configuration.AppSettingsReader();
            return appReader.GetValue(chave, typeof(string)).ToString();
        }

        public static string FileTemp
        {
            get { return GetValue("fileTemp"); }
        }
    }
}