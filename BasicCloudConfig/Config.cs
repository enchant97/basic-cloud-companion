using System.Configuration;

namespace BasicCloudConfig
{
    public static class Config
    {
        private static void UpdateValueByKey(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] == null) { config.AppSettings.Settings.Add(key, value); }
            else { config.AppSettings.Settings[key].Value = value; }
            System.Diagnostics.Debug.WriteLine("updated value by key: " + key);

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.SectionName);
            System.Diagnostics.Debug.WriteLine("saved config file: " + config.FilePath);
        }
        private static string GetValueByKey(string key, string _default = null)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value)) { return _default; }
            return value;
        }
        public static string BasicCloudUrl
        {
            get => GetValueByKey("basicCloudUrl", "http://127.0.0.1:8000");
            set => UpdateValueByKey("basicCloudUrl", value);
        }
        public static string Username
        {
            get => GetValueByKey("username");
            set => UpdateValueByKey("username", value);
        }
    }
}
