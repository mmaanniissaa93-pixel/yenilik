using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace xBot.App
{
    /// <summary>
    /// Represents a saved Silkroad account credential and preference profile.
    /// </summary>
    public class SavedAccount
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string Character { get; set; } = string.Empty;
        public string Silkroad { get; set; } = string.Empty;

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Character) && !string.IsNullOrWhiteSpace(Server))
                return $"{Username} [{Server} - {Character}]";
            if (!string.IsNullOrWhiteSpace(Server))
                return $"{Username} [{Server}]";
            return Username;
        }
    }

    /// <summary>
    /// Manages the persistence, selection, and retrieval of saved accounts.
    /// </summary>
    public static class AccountManager
    {
        public static List<SavedAccount> Accounts { get; } = new List<SavedAccount>();
        public static string SelectedAccountUsername { get; set; } = string.Empty;

        /// <summary>
        /// Saves or updates an account.
        /// </summary>
        public static void SaveAccount(SavedAccount account)
        {
            if (account == null || string.IsNullOrWhiteSpace(account.Username))
                return;

            string normalized = account.Username.Trim();
            var existing = Accounts.Find(a => a.Username.Equals(normalized, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.Password = account.Password;
                existing.Server = account.Server ?? string.Empty;
                existing.Character = account.Character ?? string.Empty;
                existing.Silkroad = account.Silkroad ?? string.Empty;
            }
            else
            {
                account.Username = normalized;
                Accounts.Add(account);
            }

            SelectedAccountUsername = normalized;
            Settings.SaveBotSettings();
        }

        /// <summary>
        /// Deletes an account by username.
        /// </summary>
        public static bool DeleteAccount(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            string normalized = username.Trim();
            int removed = Accounts.RemoveAll(a => a.Username.Equals(normalized, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                if (SelectedAccountUsername.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                {
                    SelectedAccountUsername = Accounts.Count > 0 ? Accounts[0].Username : string.Empty;
                }
                Settings.SaveBotSettings();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds a saved account by username.
        /// </summary>
        public static SavedAccount GetAccount(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return Accounts.Find(a => a.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Serializes saved accounts to a JArray for Settings.json.
        /// </summary>
        public static JArray ToJson()
        {
            JArray array = new JArray();
            foreach (var acc in Accounts)
            {
                JObject obj = new JObject
                {
                    ["Username"] = acc.Username,
                    ["Password"] = acc.Password,
                    ["Server"] = acc.Server,
                    ["Character"] = acc.Character,
                    ["Silkroad"] = acc.Silkroad
                };
                array.Add(obj);
            }
            return array;
        }

        /// <summary>
        /// Deserializes saved accounts from JArray.
        /// </summary>
        public static void FromJson(JArray array, string selected = "")
        {
            Accounts.Clear();
            if (array != null)
            {
                foreach (JObject obj in array)
                {
                    string user = (string)obj["Username"] ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(user))
                        continue;

                    SavedAccount acc = new SavedAccount
                    {
                        Username = user.Trim(),
                        Password = (string)obj["Password"] ?? string.Empty,
                        Server = (string)obj["Server"] ?? string.Empty,
                        Character = (string)obj["Character"] ?? string.Empty,
                        Silkroad = (string)obj["Silkroad"] ?? string.Empty
                    };
                    Accounts.Add(acc);
                }
            }

            SelectedAccountUsername = selected ?? string.Empty;
        }
    }
}
