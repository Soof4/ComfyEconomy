using TShockAPI;

namespace ComfyEconomy
{
    public class LogManager
    {
        public static string LogPath = Path.Combine(TShock.SavePath, "logs", "ComfyEconomy");

        public static void InitializeLogging()
        {
            if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);

            LogPath = Path.Combine(LogPath, $"{DateTime.Now.ToString("s")}.log");
        }

        public static void Log(string tag, string accName, string text)
        {
            using (StreamWriter writer = new StreamWriter(LogPath, true))
            {
                writer.WriteLine($"[{DateTime.Now.ToString("s")}] : [{tag}] : {accName} > {text}");
            }
        }
    }
}