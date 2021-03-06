﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBotShared.Json
{
    public class JsonConfig
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string master { get; set; }
        public string code { get; set; }
        public string discord { get; set; }
        public bool allowRLV { get; set; }
        public string discordGroupTarget { get; set; }
        public string[] homeRegion { get; set; }
        public bool EnableHttp { get; set; }
        public int Httpport { get; set; }
        public string Httpkey { get; set; }
        public bool HttpRequireSigned { get; set; }
        public string HttpHost { get; set; }
        public bool HttpAsCnC { get; set; }
        public bool DiscordFullServer { get; set; }
        public string DiscordClientToken { get; set; }
        public ulong DiscordServerID { get; set; }
        public int DiscordServerImHistoryHours { get; set; }
        public string DefaultSitUUID { get; set; }
    }

}
