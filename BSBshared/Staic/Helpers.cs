﻿using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BetterSecondBotShared.Static
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
    public static class helpers
    {
        public static string GetSHA1(string text)
        {
            SHA1 hash = SHA1CryptoServiceProvider.Create();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(text);
            byte[] hashBytes = hash.ComputeHash(plainTextBytes);
            string localChecksum = BitConverter.ToString(hashBytes)
            .Replace("-", "").ToLowerInvariant();
            return localChecksum;
        }
        public static string[] ParseSLurl(string url)
        {
            if (url != null)
            {
                url = url.Replace("http://maps.secondlife.com/secondlife/", "");
                string[] bits = url.Split('/');
                if (bits.Length == 4)
                {
                    return bits;
                }
            }
            return null;
        }
        public static string RegionnameFromSLurl(string url)
        {
            string[] bits = ParseSLurl(url);
            if (bits != null)
            {
                if (bits.Length == 4)
                {
                    return bits[0];
                }
            }
            return "";
        }
        public static long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
        public static bool botRequired(JsonConfig test)
        {
            if (helpers.notempty(test.userName) && helpers.notempty(test.password) && helpers.notempty(test.master) && helpers.notempty(test.code))
            {
                // required values are set
                MakeJsonConfig Testing = new MakeJsonConfig();
                string[] testingfor = new[] { "userName", "password", "code" };
                bool default_value_found = false;
                foreach (string a in testingfor)
                {
                    if (Testing.GetCommandArgTypes(a).First() == Testing.GetProp(test, a))
                    {
                        ConsoleLog.Warn("" + a + " is currently set to the default");
                        default_value_found = true;
                        break;
                    }
                }
                if (default_value_found == false)
                {
                    ConsoleLog.Status("User => " + test.userName);
                    ConsoleLog.Status("Master => " + test.master);
                }
                return !default_value_found;
            }
            else
            {
                if (helpers.notempty(test.userName) == false)
                {
                    ConsoleLog.Warn("Username is null or empty");
                }
                if (helpers.notempty(test.password) == false)
                {
                    ConsoleLog.Warn("Password is null or empty");
                }
                if (helpers.notempty(test.master) == false)
                {
                    ConsoleLog.Warn("Master is null or empty");
                }
                if (helpers.notempty(test.code) == false)
                {
                    ConsoleLog.Warn("Code is null or empty");
                }
                return false;
            }
        }
        public static string create_dirty_table(string[] items)
        {
            return create_dirty_table(items, 6);
        }
        public static string create_dirty_table(string[] items, int cols)
        {

            bool row_open = false;
            int col_counter = 0;
            StringBuilder reply = new StringBuilder();
            reply.Append("<table border='1'>");
            foreach (string A in items)
            {
                if (row_open == false)
                {
                    reply.Append("<tr>");
                    row_open = true;
                }
                reply.Append("<td>" + A + "</td>");
                col_counter++;
                if (col_counter >= cols)
                {
                    reply.Append("</tr>");
                    col_counter = 0;
                    row_open = false;
                }
            }
            while (row_open == true)
            {
                reply.Append("<td></td>");
                col_counter++;
                if (col_counter >= cols)
                {
                    reply.Append("</tr>");
                    row_open = false;
                }
            }
            reply.Append("</table>");
            return reply.ToString();
        }
        public static bool notempty(string V)
        {
            if (V != null)
            {
                if (V.Length > 0) return true;
                else return false;
            }
            else
            {
                return false;
            }
        }
        public static bool notempty(string[] V)
        {
            if (V != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
