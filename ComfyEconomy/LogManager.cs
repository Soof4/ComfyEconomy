using TShockAPI;


namespace ComfyEconomy
{
    public class LogManager
    {
        public static string LogPath = Path.Combine(TShock.SavePath, "logs", "ComfyEconomy");
        private static StreamWriter? Writer;

        public static void InitializeLogging()
        {
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            LogPath = Path.Combine(LogPath, $"{DateTime.Now.ToString("s")}.log");
            Writer = new StreamWriter(LogPath, true);
        }
        
        public static void Log(string tag, string accName, string text)
        {
            using (StreamWriter Writer = new StreamWriter(LogPath, true))
            {
                Writer.WriteLine($"[{DateTime.Now.ToString("s")}] : [{tag}] : {accName} > {text}");
            }
        }
    }
}