using System;
using System.Collections.Generic;
using System.Linq;

namespace xBot.App
{
    /// <summary>
    /// Script komutlarının tek kaynağı. Çalıştırıcı ve Script Creator aynı sözlüğü kullanır.
    /// Virgüllü biçim, boşluk içeren skill/item/teleport adlarını korur.
    /// </summary>
    public static class ScriptCommandCatalog
    {
        private static readonly List<ScriptCommandDefinition> m_definitions = new List<ScriptCommandDefinition>
        {
            new ScriptCommandDefinition("move", new[] { "walk" }, "move, X, Y", "Belirtilen koordinata yürü.", 2, 5, "X", "Y / Region,X,Y,Z"),
            new ScriptCommandDefinition("store", null, "store, NPC_CODE", "Kasaba depolama adımını çalıştır.", 0, 1, "NPC kodu", ""),
            new ScriptCommandDefinition("buy", null, "buy, NPC_CODE", "Kasaba satın alma adımını çalıştır.", 0, 1, "NPC kodu", ""),
            new ScriptCommandDefinition("repair", null, "repair, NPC_CODE", "Kasaba tamir adımını çalıştır.", 0, 1, "NPC kodu", ""),
            new ScriptCommandDefinition("wait", null, "wait, milliseconds", "Scripti kesilebilir biçimde beklet.", 1, 1, "Milisaniye", ""),
            new ScriptCommandDefinition("cast", null, "cast, skill name", "İsim, server adı veya ID ile skill kullan.", 1, 1, "Skill adı / ID", ""),
            new ScriptCommandDefinition("use", null, "use, item name", "İsim, server adı veya slot ile çantadaki itemi kullan.", 1, 1, "Item adı / slot", ""),
            new ScriptCommandDefinition("teleport", null, "teleport, source, destination", "Kaynak ve hedef adı/ID ile teleport kullan.", 2, 2, "Kaynak teleport", "Hedef teleport"),
            new ScriptCommandDefinition("DoBlacksmith", null, "DoBlacksmith", "Demirci işlemlerini uygula: tamir ve ayarlı satış.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoHerbalist", null, "DoHerbalist", "Herbalist işlemlerini uygula: iksir/pill/scroll alımı.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoStable", null, "DoStable", "Stable NPC oturumunu doğrula ve ayarlı işlemleri uygula.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoStorage", null, "DoStorage", "Storage kurallarına göre eşya ve altın depola.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoStorageStore", null, "DoStorageStore", "Yalnızca storage kurallarındaki eşyaları depola.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoStorageTake", null, "DoStorageTake", "Take işaretli eşyaları kişisel storage'dan al.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoGuildStorage", null, "DoGuildStorage", "Guild storage kurallarına göre eşya ve altın depola.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoGuildStorageStore", null, "DoGuildStorageStore", "Yalnızca StoreGuild işaretli eşyaları guild storage'a koy.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoGuildStorageTake", null, "DoGuildStorageTake", "TakeGuild işaretli eşyaları guild storage'dan al.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoGroceryTrader", null, "DoGroceryTrader", "Grocery işlemlerini uygula ve gerekiyorsa ok/bolt al.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoProtectorTrader", null, "DoProtectorTrader", "Protector NPC'de ayarlı satış işlemlerini uygula.", 0, 0, "", ""),
            new ScriptCommandDefinition("DoJupiter", null, "DoJupiter", "Jupiter birleşik demirci/herbalist işlemlerini uygula.", 0, 0, "", ""),
            new ScriptCommandDefinition("recall", null, "recall", "Aktif toplama petini geri çağır.", 0, 0, "", ""),
            new ScriptCommandDefinition("mount", null, "mount, fellow|transport", "Çağrılmış fellow veya transport petine bin.", 0, 1, "Pet türü (fellow/transport)", ""),
            new ScriptCommandDefinition("dismount", null, "dismount", "Fellow/transporttan in; normal atı sonlandır.", 0, 0, "", ""),
            new ScriptCommandDefinition("killhorse", null, "killhorse", "Aktif atı veya transportu tamamen sonlandır.", 0, 0, "", ""),
            new ScriptCommandDefinition("stop", null, "stop", "Botu ve aktif scripti durdur.", 0, 0, "", ""),
            new ScriptCommandDefinition("disconnect", null, "disconnect", "Bağlantıyı güvenli biçimde kapat.", 0, 0, "", "")
        };

        public static IList<ScriptCommandDefinition> Definitions
        {
            get { return m_definitions.AsReadOnly(); }
        }

        public static bool TryParse(string line, out ScriptCommandInvocation invocation, out string error)
        {
            invocation = null;
            error = null;
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                error = "Komut satırı boş.";
                return false;
            }

            string[] tokens = Tokenize(line);
            if (tokens.Length == 0)
            {
                error = "Komut bulunamadı.";
                return false;
            }

            string requestedName = tokens[0].Trim();
            ScriptCommandDefinition definition = Find(requestedName);
            if (definition == null)
            {
                error = "Bilinmeyen script komutu: " + requestedName;
                return false;
            }

            string[] arguments = tokens.Skip(1).Select(value => value.Trim()).ToArray();
            if (arguments.Length < definition.MinimumArguments || arguments.Length > definition.MaximumArguments)
            {
                error = string.Format("{0}: {1}-{2} parametre bekleniyor. Sözdizimi: {3}",
                    requestedName, definition.MinimumArguments, definition.MaximumArguments, definition.Syntax);
                return false;
            }
            if (arguments.Any(string.IsNullOrWhiteSpace))
            {
                error = requestedName + ": boş parametre kullanılamaz. Sözdizimi: " + definition.Syntax;
                return false;
            }
            if (definition.Name == "move")
            {
                if (arguments.Length != 2 && arguments.Length != 4 && arguments.Length != 5)
                {
                    error = "MOVE: 2D, Region/X/Y/Z veya .rbs X/Y/Z/secX/secY biçimi bekleniyor.";
                    return false;
                }
                int numericValue;
                if (arguments.Any(value => !int.TryParse(value, out numericValue)))
                {
                    error = "MOVE: bütün koordinatlar sayı olmalı.";
                    return false;
                }
            }
            if (definition.Name == "wait")
            {
                int milliseconds;
                if (!int.TryParse(arguments[0], out milliseconds) || milliseconds < 0 || milliseconds > 3600000)
                {
                    error = "WAIT: süre 0-3600000 ms arasında olmalı.";
                    return false;
                }
            }
            if (definition.Name == "mount" && arguments.Length == 1
                && !arguments[0].Equals("fellow", StringComparison.OrdinalIgnoreCase)
                && !arguments[0].Equals("transport", StringComparison.OrdinalIgnoreCase))
            {
                error = "MOUNT: pet türü fellow veya transport olmalı.";
                return false;
            }

            invocation = new ScriptCommandInvocation(definition, arguments, line);
            return true;
        }

        public static ScriptCommandDefinition Find(string commandOrAlias)
        {
            if (string.IsNullOrWhiteSpace(commandOrAlias))
                return null;
            return m_definitions.FirstOrDefault(definition => definition.Matches(commandOrAlias));
        }

        public static string Format(ScriptCommandDefinition definition, params string[] arguments)
        {
            if (definition == null)
                return string.Empty;
            List<string> parts = new List<string> { definition.Name };
            if (arguments != null)
                parts.AddRange(arguments.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
            return string.Join(", ", parts);
        }

        private static string[] Tokenize(string line)
        {
            if (line.IndexOf(',') >= 0)
                return line.Split(new[] { ',' }, StringSplitOptions.None).Select(value => value.Trim()).ToArray();
            return line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public sealed class ScriptCommandDefinition
    {
        public string Name { get; private set; }
        public string[] Aliases { get; private set; }
        public string Syntax { get; private set; }
        public string Description { get; private set; }
        public int MinimumArguments { get; private set; }
        public int MaximumArguments { get; private set; }
        public string FirstParameterLabel { get; private set; }
        public string SecondParameterLabel { get; private set; }

        public ScriptCommandDefinition(string name, string[] aliases, string syntax, string description,
            int minimumArguments, int maximumArguments, string firstParameterLabel, string secondParameterLabel)
        {
            Name = name;
            Aliases = aliases ?? new string[0];
            Syntax = syntax;
            Description = description;
            MinimumArguments = minimumArguments;
            MaximumArguments = maximumArguments;
            FirstParameterLabel = firstParameterLabel;
            SecondParameterLabel = secondParameterLabel;
        }

        public bool Matches(string value)
        {
            return Name.Equals(value, StringComparison.OrdinalIgnoreCase)
                || Aliases.Any(alias => alias.Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class ScriptCommandInvocation
    {
        public ScriptCommandDefinition Definition { get; private set; }
        public string Command { get { return Definition.Name; } }
        public string[] Arguments { get; private set; }
        public string OriginalLine { get; private set; }

        public ScriptCommandInvocation(ScriptCommandDefinition definition, string[] arguments, string originalLine)
        {
            Definition = definition;
            Arguments = arguments ?? new string[0];
            OriginalLine = originalLine;
        }

        public string[] ToLegacyTokens()
        {
            string[] tokens = new string[Arguments.Length + 1];
            tokens[0] = Command;
            Array.Copy(Arguments, 0, tokens, 1, Arguments.Length);
            return tokens;
        }
    }
}
