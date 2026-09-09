using System;
using System.Collections.Generic;

namespace xBot.App
{
    public sealed class ConsignmentRule
    {
        public string ItemName { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public ushort Quantity { get; set; } = 1;
        public ulong Price { get; set; }
    }

    public sealed class ConsignmentRegistrationCandidate
    {
        public string ItemName { get; set; } = "";
        public byte InventorySlot { get; set; }
        public ushort Quantity { get; set; }
        public ulong Price { get; set; }
        public uint RentableId { get; set; }
        public uint ItemId { get; set; }
    }

    public static class ConsignmentPolicy
    {
        public const ulong MaximumPrice = 999999999999UL;

        public static string ValidateRule(ConsignmentRule rule)
        {
            if (rule == null)
                return "Kural boş.";
            if (string.IsNullOrWhiteSpace(rule.ItemName))
                return "Item adı boş olamaz.";
            if (rule.Quantity == 0)
                return "Miktar en az 1 olmalı.";
            if (rule.Price == 0)
                return "Fiyat en az 1 gold olmalı.";
            if (rule.Price > MaximumPrice)
                return "Fiyat izin verilen üst sınırı aşıyor.";
            return "";
        }

        public static ConsignmentRegistrationCandidate CreateCandidate(
            ConsignmentRule rule, byte inventorySlot, ushort inventoryQuantity)
        {
            if (!string.IsNullOrEmpty(ValidateRule(rule)) || !rule.Enabled || inventoryQuantity == 0)
                return null;

            return new ConsignmentRegistrationCandidate
            {
                ItemName = rule.ItemName.Trim(),
                InventorySlot = inventorySlot,
                // Eski GS sürümlerindeki 0x7508 miktar açığına karşı sunucuya
                // asla karakterde bulunandan daha yüksek bir adet yazma.
                Quantity = Math.Min(rule.Quantity, inventoryQuantity),
                Price = rule.Price
            };
        }

        public static bool MatchesItemPattern(string pattern, string itemName, string serverName)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;
            return WildcardMatch(pattern.Trim(), itemName ?? "")
                || WildcardMatch(pattern.Trim(), serverName ?? "");
        }

        private static bool WildcardMatch(string pattern, string value)
        {
            int p = 0, v = 0, star = -1, retry = 0;
            while (v < value.Length)
            {
                if (p < pattern.Length && (pattern[p] == '?'
                    || char.ToUpperInvariant(pattern[p]) == char.ToUpperInvariant(value[v])))
                {
                    p++; v++; continue;
                }
                if (p < pattern.Length && pattern[p] == '*')
                {
                    star = p++;
                    retry = v;
                    continue;
                }
                if (star >= 0)
                {
                    p = star + 1;
                    v = ++retry;
                    continue;
                }
                return false;
            }
            while (p < pattern.Length && pattern[p] == '*') p++;
            return p == pattern.Length;
        }

        public static bool TryParseListingPayload(byte[] payload, out List<ConsignmentListing> listings, out string error)
        {
            listings = new List<ConsignmentListing>();
            error = "";
            if (payload == null || payload.Length < 2)
            {
                error = "Liste cevabı çok kısa.";
                return false;
            }
            if (payload[0] != 1)
            {
                error = payload[0] == 2 && payload.Length >= 3
                    ? "Sunucu hata kodu: 0x" + BitConverter.ToUInt16(payload, 1).ToString("X4")
                    : "Başarısız sonuç: " + payload[0];
                return false;
            }

            int count = payload[1];
            const int recordLength = 41;
            int expected = 2 + count * recordLength;
            if (payload.Length != expected)
            {
                error = "Beklenen " + expected + " bayt, alınan " + payload.Length + " bayt.";
                return false;
            }

            int offset = 2;
            for (int i = 0; i < count; i++)
            {
                var listing = new ConsignmentListing();
                listing.ConsignmentId = ReadUInt(payload, ref offset);
                listing.Status = payload[offset++];
                listing.ItemId = ReadUInt(payload, ref offset);
                listing.Quantity = ReadUInt(payload, ref offset);
                listing.DepositedGold = ReadULong(payload, ref offset);
                listing.SellingFee = ReadULong(payload, ref offset);
                listing.Price = ReadULong(payload, ref offset);
                listing.EndDate = ReadUInt(payload, ref offset);
                listings.Add(listing);
            }
            return true;
        }

        private static uint ReadUInt(byte[] bytes, ref int offset)
        {
            uint value = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            return value;
        }

        private static ulong ReadULong(byte[] bytes, ref int offset)
        {
            ulong value = BitConverter.ToUInt64(bytes, offset);
            offset += 8;
            return value;
        }
    }

    public sealed class ConsignmentListing
    {
        public uint ConsignmentId { get; set; }
        public byte Status { get; set; }
        public uint ItemId { get; set; }
        public uint Quantity { get; set; }
        public ulong DepositedGold { get; set; }
        public ulong SellingFee { get; set; }
        public ulong Price { get; set; }
        public uint EndDate { get; set; }
        public bool IsExpired { get { return Status == 1; } }
        public bool IsSold { get { return Status == 2; } }
    }
}
