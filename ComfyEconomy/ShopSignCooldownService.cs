using TShockAPI;

namespace ComfyEconomy
{
    public static class ShopSignCooldownService
    {
        private class CooldownInfo
        {
            public int SignIndex { get; set; }
            public string SignText { get; set; }
            public string? PlayerName { get; set; }
            public DateTime LastUseTime { get; set; }
            public TimeSpan Cooldown { get; set; }


            public CooldownInfo(int signIndex, string signText, string? playerName, DateTime lastUseTime)
            {
                SignIndex = signIndex;
                SignText = signText;
                PlayerName = playerName;
                LastUseTime = lastUseTime;
                Cooldown = ParseCooldown(signText);
            }

            public bool HasTimePassed()
            {
                TimeSpan deltaTime = DateTime.UtcNow - LastUseTime;
                return deltaTime >= Cooldown;
            }

            private TimeSpan ParseCooldown(string signText)
            {
                string text = signText.Split('\n').First();   // 0       1         2..
                string[] timewords = text.Split(' ')[2..];    // SignTag Cooldown: 5h 2m 1s
                                                              // SignTag GlobalCooldown: 5h
                                                              // 0       1               2..
                int hours = 0;
                int mins = 0;
                int secs = 0;

                foreach (string tw in timewords)
                {
                    if (tw.EndsWith('h'))
                    {
                        hours = int.Parse(tw[..tw.IndexOf('h')]);
                    }
                    else if (tw.EndsWith('m'))
                    {
                        mins = int.Parse(tw[..tw.IndexOf('m')]);
                    }
                    else if (tw.EndsWith('s'))
                    {
                        secs = int.Parse(tw[..tw.IndexOf('s')]);
                    }
                }

                return new TimeSpan(hours: hours, minutes: mins, seconds: secs);
            }
        }

        private static List<CooldownInfo> _cooldownInfos = new List<CooldownInfo>();

        public static bool IsInCooldownForPlayer(TSPlayer player, int signIndex, string signText)
        {
            if (!signText.Split('\n').First().Contains("Cooldown")) return false;

            for (int i = 0; i < _cooldownInfos.Count; i++)
            {
                CooldownInfo info = _cooldownInfos[i];

                if (info.HasTimePassed())
                {
                    _cooldownInfos.RemoveAt(i);
                    i--;
                }
                else if (info.SignIndex == signIndex && (info.PlayerName == player.Name || info.PlayerName == null) && info.SignText == signText)
                {
                    return true;
                }
            }

            bool isGlobal = signText.Split('\n').First().Split(' ')[1].StartsWith("Global");

            _cooldownInfos.Add(new CooldownInfo(signIndex, signText, isGlobal ? null : player.Name, DateTime.UtcNow));

            return false;
        }

    }
}
