using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ComfyEconomy {
    public class Config {
        public int MineRefillIntervalInMins = 60;
        public int MinePostponeMins = 5;
        public bool EnableLogs = true;
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
