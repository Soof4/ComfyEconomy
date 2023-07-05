using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComfyEconomy {
    public class Config {
        public int MineRefillIntervalInMins = 60;
        public int ShopSignControlIntervalInMins = 5;
        public void Write() {
            File.WriteAllText(ComfyEconomy.configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read() {
            if (!File.Exists(ComfyEconomy.configPath)) {
                return new Config();
            }
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ComfyEconomy.configPath));
        }
    }
}
