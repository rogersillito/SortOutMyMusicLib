using System.Configuration;

namespace SortOutMyMusicLib.Lib
{
    public interface IConfigReader
    {
        string GetAppSetting(string settingKey);
    }

    public class ConfigReader : IConfigReader
    {
        public string GetAppSetting(string settingKey)
        {
            return ConfigurationManager.AppSettings[settingKey];
        }
    }
}