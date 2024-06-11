using IL.Terraria.GameContent;
using Newtonsoft.Json;
using TerrariaApi.Server;
using TShockAPI;

namespace ComfyEconomy
{
    public static class UpdateManager
    {
        public static async Task<Version?> RequestLatestVersion()
        {
            string url = "https://api.github.com/repos/Soof4/ComfyEconomy/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); // Set a user agent header

                try
                {
                    var response = await client.GetStringAsync(url);
                    dynamic? latestRelease = JsonConvert.DeserializeObject<dynamic>(response);

                    if (latestRelease == null) return null;

                    string tag = latestRelease.tag_name;

                    tag = tag.Trim('v');
                    string[] nums = tag.Split('.');

                    Version version = new Version(int.Parse(nums[0]),
                                                  int.Parse(nums[1]),
                                                  int.Parse(nums[2])
                                                  );
                    return version;
                }
                catch
                {
                    Console.WriteLine("An error occured during checking for ComfyEconomy update.");
                }
            }

            return null;
        }

        public static async Task<bool> IsUpToDate(TerrariaPlugin plugin)
        {
            Version? latestVersion = await RequestLatestVersion();
            Version curVersion = plugin.Version;

            return latestVersion != null && curVersion == latestVersion;
        }
    
        public static async void CheckUpdateVerbose(TerrariaPlugin plugin) {
            TSPlayer.Server.SendInfoMessage("Checking for ComfyEconomy updates...");
            bool isUpToDate = await IsUpToDate(plugin);

            if (isUpToDate) {
                TSPlayer.Server.SendSuccessMessage("ComfyEconomy is up to date!");
            }
            else {
                TSPlayer.Server.SendErrorMessage("ComfyEconomy is not up to date.\n" +
                "Please visit https://github.com/Soof4/ComfyEconomy/releases/latest to download the latest version.");
            }
        }
    }
}