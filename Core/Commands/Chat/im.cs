﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;

namespace BSB.Commands.Chat
{
    class IM : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Avatar", "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]","Message" }; } }
        public override string Helpfile { get { return "Makes the bot send a IM to an avatar<br/>Example: IM|||289c3e36-69b3-40c5-9229-0c6a5d230766~#~Hello Mad I am a bot"; } }
        public override int MinArgs { get { return 2; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID target_av) == true)
                {
                    bot.GetClient.Self.InstantMessage(target_av, args[1]);
                    return true;
                }
                else
                {
                    return Failed("Not a vaild UUID");
                }
            }
            return false;
        }
    }
}
