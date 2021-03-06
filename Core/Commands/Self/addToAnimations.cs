﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Self
{
    public class AddToAllowAnimations : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar name"}; } }
        public override string Helpfile { get { return "Toggles if animation requests from this avatar (used for remote poseballs) are accepted<br/>Case sensitive also requires Lastname"; } }
        public override int MinArgs { get { return 1; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                bot.ToggleAutoAcceptAnimations(args[0]);
                return true;
            }
            return false;
        }
    }
}
