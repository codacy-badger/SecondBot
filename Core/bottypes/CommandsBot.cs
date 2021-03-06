﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using OpenMetaverse.Assets;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;

namespace BSB.bottypes
{
    public abstract class CommandsBot : AVstorageBot
    {
        public Commands.CoreCommandsInterface GetCommandsInterface { get { return CommandsInterface; } }
        protected Commands.CoreCommandsInterface CommandsInterface;
        public CommandsBot()
        {
            CommandsInterface = new Commands.CoreCommandsInterface(this);
        }
        protected Dictionary<string, string> notecards_content = new Dictionary<string, string>();
        protected Dictionary<UUID, long> group_invite_lockout_timer = new Dictionary<UUID, long>();
        protected long last_cleanup_commands;

        public override string GetStatus()
        {
            long dif = helpers.UnixTimeNow() - last_cleanup_commands;
            if (dif > 30)
            {
                last_cleanup_commands = helpers.UnixTimeNow();
                PurgeOldGroupinviteLockouts();
            }
            CommandsInterface.StatusTick();
            return base.GetStatus();
        }
        public bool GetAllowGroupInvite(UUID avatar)
        {
            return !group_invite_lockout_timer.ContainsKey(avatar);
        }

        public void GroupInviteLockoutArm(UUID avatar)
        {
            if (group_invite_lockout_timer.ContainsKey(avatar) == false)
            {
                group_invite_lockout_timer.Add(avatar,0);
            }
            group_invite_lockout_timer[avatar] = helpers.UnixTimeNow();
        }
        protected void PurgeOldGroupinviteLockouts()
        {
            long now = helpers.UnixTimeNow();
            List<UUID> clear_locks = new List<UUID>();
            foreach(KeyValuePair<UUID,long> Glock in group_invite_lockout_timer)
            {
                long dif = now - Glock.Value;
                if(dif >= 120)
                {
                    clear_locks.Add(Glock.Key);
                }
            }
            foreach(UUID glock in clear_locks)
            {
                group_invite_lockout_timer.Remove(glock);
            }
        }

        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            if (reconnect == false)
            {
                CommandsInterface = new Commands.CoreCommandsInterface(this);
            }
        }

        protected override void CoreCommandLib(UUID fromUUID, bool from_master, string command, string arg, string signing_code)
        {
            if (CommandsInterface != null)
            {
                if (signing_code != "")
                {
                    string raw = "" + command + "" + arg + "" + myconfig.code + "";
                    string hashcheck = helpers.GetSHA1(raw);
                    if (hashcheck == signing_code)
                    {
                        // signed commands
                        CommandsInterface.Call(command, arg);
                    }
                }
                else if (from_master == true)
                {
                    CommandsInterface.Call(command, arg);
                }
            }
        }
        public void NotecardAddContent(string target_notecard_storage_id, string content)
        {
            NotecardAddContent(target_notecard_storage_id, content, true, "\n\r");
        }
        public void NotecardAddContent(string target_notecard_storage_id, string content, bool add_newline)
        {
            NotecardAddContent(target_notecard_storage_id, content, add_newline, "\n\r");
        }
        public void NotecardAddContent(string target_notecard_storage_id,string content,bool add_newline,string newlinevalue)
        {
            if(notecards_content.ContainsKey(target_notecard_storage_id) == false)
            {
                notecards_content.Add(target_notecard_storage_id, "");
            }
            if (add_newline == true)
            {
                content = "" + newlinevalue + "" + content;
            }
            notecards_content[target_notecard_storage_id] = notecards_content[target_notecard_storage_id] + ""+content;
        }
        public string GetNotecardContent(string target_notecard_storage_id)
        {
            if (notecards_content.ContainsKey(target_notecard_storage_id) == true)
            {
                return notecards_content[target_notecard_storage_id];
            }
            return null;
        }

        public void ClearNotecardStorage(string target_notecard_storage_id)
        {
            if (notecards_content.ContainsKey(target_notecard_storage_id) == true)
            {
                notecards_content.Remove(target_notecard_storage_id);
            }
        }

        public bool SendNotecard(string name, string content, UUID sendToUUID)
        {
            bool returnstatus = true;
            name = name + " " + DateTime.Now;
            Client.Inventory.RequestCreateItem(
                Client.Inventory.FindFolderForType(AssetType.Notecard),
                name,
                name + " Created via SecondBot notecard API",
                AssetType.Notecard,
                UUID.Random(),
                InventoryType.Notecard,
                PermissionMask.All,
                (bool Success, InventoryItem item) =>
                {
                    if (Success)
                    {
                        AssetNotecard empty = new AssetNotecard { BodyText = "\n" };
                        empty.Encode();
                        Client.Inventory.RequestUploadNotecardAsset(empty.AssetData, item.UUID,
                        (bool emptySuccess, string emptyStatus, UUID emptyItemID, UUID emptyAssetID) =>
                        {
                            if (emptySuccess)
                            {
                                empty.BodyText = content;
                                empty.Encode();
                                Client.Inventory.RequestUploadNotecardAsset(empty.AssetData, emptyItemID,
                                (bool finalSuccess, string finalStatus, UUID finalItemID, UUID finalID) =>
                                {
                                    if (finalSuccess)
                                    {
                                        Client.Inventory.GiveItem(finalItemID, name, AssetType.Notecard, sendToUUID, false);
                                    }
                                    else
                                    {
                                        returnstatus = false;
                                        ConsoleLog.Warn("Unable to request notecard upload");
                                    }

                                });
                            }
                            else
                            {
                                ConsoleLog.Crit("The fuck empty success notecard create");
                                returnstatus = false;
                            }
                        });
                    }
                    else
                    {
                        ConsoleLog.Warn("Unable to find default notecards folder");
                        returnstatus = false;
                    }
                }
            );
            return returnstatus;
        }
    }
}
