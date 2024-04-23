using Newtonsoft.Json;
using TShockAPI;

namespace ComfyEconomy
{
    public class Configuration
    {
        public static string ConfigPath = Path.Combine(TShock.SavePath, "ComfyEconomyConfig.json");
        public int MineRefillIntervalInMins = 60;
        public int MinePostponeMins = 5;
        public bool EnableLogs = true;

        public static Configuration Reload()
        {
            Configuration? c = null;

            if (File.Exists(ConfigPath)) c = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigPath));

            if (c == null)
            {
                c = new Configuration();
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(c, Formatting.Indented));
            }

            return c;
        }
    }
}
