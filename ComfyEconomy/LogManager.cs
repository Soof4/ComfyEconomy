
namespace ComfyEconomy {
    public class LogManager {
        private static StreamWriter writer = new StreamWriter(ComfyEconomy.LogPath, true);

        public static void Log(string tag, string accName, string text) {
            using (StreamWriter writer = new StreamWriter(ComfyEconomy.LogPath, true)) {
                writer.WriteLine($"[{DateTime.Now.ToString("s")}] : [{tag}] : {accName} > {text}");
            }
        }
    }
}