using Newtonsoft.Json;
using TShockAPI;

namespace ComfyEconomy
{
    public class Config
    {
        public static string ConfigPath = Path.Combine(TShock.SavePath, "ComfyEconomyConfig.json");
        public int MineRefillIntervalInMins = 60;
        public int MinePostponeMins = 5;
        public bool EnableLogs = true;

        public static Config Reload()
        {
            Config? c = null;

            if (File.Exists(ConfigPath))
            {
                c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }

            if (c == null)
            {
                c = new Config();
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(c, Formatting.Indented));
            }

            return c;
        }
    }
}
