using System;
using System.Collections.Generic;
using System.Text;

namespace xBot.App
{
    public enum QuestCompletionAction
    {
        None,
        ReturnTown,
        RunScript,
        TurnInAtNpc
    }

    public enum QuestNpcOperation
    {
        Accept,
        TurnIn
    }

    public sealed class QuestTalkSnapshot
    {
        public byte Type { get; set; }
        public string Header { get; set; } = "";
        public IList<string> Choices { get; set; } = new List<string>().AsReadOnly();
        public string Footer { get; set; } = "";
        public uint ReferenceId { get; set; }
        public string RawHex { get; set; } = "";
        public string Error { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public sealed class QuestRewardSelection
    {
        public uint QuestId { get; set; }
        public byte Selection { get; set; }
        public uint RewardId { get; set; }
        public bool HasSelectableReward { get; set; }
        public string Error { get; set; } = "";
    }

    /// <summary>Pure quest completion decisions, kept separate for regression tests.</summary>
    public static class QuestAutomationPolicy
    {
        /// <summary>
        /// Parses the server's 0x30D4 QUEST_TALK payload. The layout was verified
        /// against the game client: type + ASCII header; types 4/5 contain a
        /// counted ASCII choice list, and type 5 ends with ASCII + UInt32.
        /// Malformed data is returned as a diagnostic snapshot instead of thrown.
        /// </summary>
        public static QuestTalkSnapshot ParseTalkPacket(byte[] raw)
        {
            QuestTalkSnapshot result = new QuestTalkSnapshot
            {
                Timestamp = DateTime.Now,
                RawHex = ToHex(raw)
            };
            try
            {
                if (raw == null || raw.Length == 0) throw new FormatException("boş paket");
                int offset = 0;
                result.Type = ReadByte(raw, ref offset);
                result.Header = ReadAscii(raw, ref offset);
                List<string> choices = new List<string>();
                if (result.Type == 4 || result.Type == 5)
                {
                    byte count = ReadByte(raw, ref offset);
                    for (int i = 0; i < count; i++) choices.Add(ReadAscii(raw, ref offset));
                }
                result.Choices = choices.AsReadOnly();
                if (result.Type == 5)
                {
                    result.Footer = ReadAscii(raw, ref offset);
                    result.ReferenceId = ReadUInt32(raw, ref offset);
                }
                if (offset != raw.Length)
                    result.Error = "paket sonunda " + (raw.Length - offset) + " okunmamış bayt";
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            return result;
        }

        public static int FindTalkChoice(IList<string> choices, string questNameString)
        {
            if (choices == null || string.IsNullOrWhiteSpace(questNameString)) return 0;
            for (int i = 0; i < choices.Count; i++)
                if (string.Equals(choices[i], questNameString, StringComparison.OrdinalIgnoreCase))
                    return i + 1; // only the one-based UI position; not the wire action code
            return 0;
        }

        public static byte ResolveTalkActionCode(byte talkType, IList<string> choices, string questNameString)
        {
            if (talkType != 4 || FindTalkChoice(choices, questNameString) == 0) return 0;
            // Verified in this client's quest-dialog builder at 0x006FC7B1:
            // dynamic entry action = 5 + (entry has the standard "SN_" prefix).
            return questNameString.StartsWith("SN_", StringComparison.OrdinalIgnoreCase) ? (byte)6 : (byte)5;
        }

        public static QuestRewardSelection ParseRewardSelection(byte[] raw)
        {
            QuestRewardSelection result = new QuestRewardSelection();
            try
            {
                if (raw == null || (raw.Length != 5 && raw.Length != 9))
                    throw new FormatException("0x7515 gövdesi 5 veya 9 bayt olmalı");
                int offset = 0;
                result.QuestId = ReadUInt32(raw, ref offset);
                result.Selection = ReadByte(raw, ref offset);
                result.HasSelectableReward = result.Selection != 0;
                if (result.HasSelectableReward)
                {
                    if (raw.Length != 9) throw new FormatException("seçilebilir 0x7515 ödülü 9 bayt olmalı");
                    result.RewardId = ReadUInt32(raw, ref offset);
                }
                else if (raw.Length != 5)
                {
                    throw new FormatException("sabit 0x7515 ödülü 5 bayt olmalı");
                }
            }
            catch (Exception ex) { result.Error = ex.Message; }
            return result;
        }

        public static bool IsReadyToTurnIn(byte questState)
        {
            // vSRO QuestStatus: 2=completed/not supplied, 8=user-completed/not supplied.
            return questState == 2 || questState == 8;
        }

        public static QuestNpcOperation? ResolveNpcOperation(bool isActive, byte questState,
            bool autoAccept, bool autoTurnIn, bool completedThisSession,
            bool repeatIfAvailable, bool isEventClaim)
        {
            if (isActive)
                return autoTurnIn && IsReadyToTurnIn(questState)
                    ? QuestNpcOperation.TurnIn : (QuestNpcOperation?)null;
            if (completedThisSession && !repeatIfAvailable) return null;
            if (isEventClaim)
                return autoTurnIn ? QuestNpcOperation.TurnIn : (QuestNpcOperation?)null;
            return autoAccept ? QuestNpcOperation.Accept : (QuestNpcOperation?)null;
        }

        public static bool ShouldQueue(bool enabled, QuestCompletionAction action,
            byte questState, bool alreadyHandled)
        {
            return enabled && action != QuestCompletionAction.None
                && IsReadyToTurnIn(questState) && !alreadyHandled;
        }

        public static string CompletionKey(uint questId, byte questState)
        {
            return questId + ":" + questState;
        }

        private static byte ReadByte(byte[] raw, ref int offset)
        {
            if (offset >= raw.Length) throw new FormatException("beklenmeyen paket sonu");
            return raw[offset++];
        }

        private static string ReadAscii(byte[] raw, ref int offset)
        {
            if (offset + 2 > raw.Length) throw new FormatException("ASCII uzunluğu eksik");
            int length = raw[offset] | (raw[offset + 1] << 8);
            offset += 2;
            if (offset + length > raw.Length) throw new FormatException("ASCII içeriği eksik");
            // QUEST_TALK strings are server-name/text-reference identifiers and
            // are ASCII on vSRO media. Avoid the optional .NET codepage provider.
            string value = Encoding.ASCII.GetString(raw, offset, length);
            offset += length;
            return value;
        }

        private static uint ReadUInt32(byte[] raw, ref int offset)
        {
            if (offset + 4 > raw.Length) throw new FormatException("UInt32 içeriği eksik");
            uint value = (uint)(raw[offset] | (raw[offset + 1] << 8)
                | (raw[offset + 2] << 16) | (raw[offset + 3] << 24));
            offset += 4;
            return value;
        }

        private static string ToHex(byte[] raw)
        {
            if (raw == null || raw.Length == 0) return "";
            StringBuilder value = new StringBuilder(raw.Length * 3 - 1);
            for (int i = 0; i < raw.Length; i++)
            {
                if (i > 0) value.Append(' ');
                value.Append(raw[i].ToString("X2"));
            }
            return value.ToString();
        }
    }
}
