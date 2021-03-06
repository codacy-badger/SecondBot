﻿using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{
    public class GetParcelFlags : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Mixed","String" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply [Channel|IM uuid|http url]", "[Repeatable] flag name" }; } }
        public override int MinArgs { get { return 1; } }
        public override string Helpfile { get { return "Returns the value of the parcel flags (At the parcel on the bot is currently on)<br/>" +
                    "requested on [ARG 2+] via [ARG 1] smart reply target<br/>" +
                    "If you request multiple Flags they are split with CSV<br/>You can also get all the flags skipping [ARG 2]<br/><br/>" +
                    "" + helpers.create_dirty_table(parcel_static.get_flag_names()) + "<br/><br/>Example: getparcelflag|||0~#~ForSale<br/>Example: getparcelflag|||12"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                Dictionary<string, ParcelFlags> flags = parcel_static.get_flags_list();
                List<string> get_flags = new List<string>();
                List<string> otherargs = new List<string>(args);
                string target = otherargs[0];
                otherargs.RemoveAt(0);
                if (otherargs.Count == 0)
                {
                    otherargs.Add("ALL");
                }
                if (otherargs[0] == "ALL")
                {
                    foreach (string A in flags.Keys)
                    {
                        get_flags.Add(A);
                    }
                }
                else
                {
                    foreach (string a in otherargs)
                    {
                        if (flags.ContainsKey(a) == true)
                        {
                            get_flags.Add(a);
                        }
                        else
                        {
                            ConsoleLog.Warn("Flag: " + a + " is unknown");
                        }
                    }
                }

                if (get_flags.Count > 0)
                {
                    int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                    Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                    StringBuilder reply = new StringBuilder();
                    string addon = "";
                    foreach (string cfg in get_flags)
                    {
                        if (flags.ContainsKey(cfg) == true)
                        {
                            reply.Append(addon);
                            reply.Append(cfg);
                            reply.Append("=");
                            reply.Append(p.Flags.HasFlag(flags[cfg]).ToString());
                            if(addon != ",") addon = ",";
                        }
                    }
                    return bot.GetCommandsInterface.SmartCommandReply(target, reply.ToString(), CommandName);
                }
                else
                {
                    return Failed("No accepted flags");
                }
            }
            return false;
        }
    }
}
