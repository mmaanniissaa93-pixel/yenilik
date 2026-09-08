namespace xBot.App
{
    /// <summary>
    /// Envanter ile kişisel/guild storage arasındaki 0x7034 hareket gövdesi.
    /// Çapraz-container hareketlerinde miktar yoktur: type + src + dst + NPC UID.
    /// </summary>
    public static class StorageMovementPolicy
    {
        public static byte[] BuildCrossContainerPayload(byte movementType, byte sourceSlot,
            byte destinationSlot, uint npcUniqueID)
        {
            return new[]
            {
                movementType,
                sourceSlot,
                destinationSlot,
                (byte)npcUniqueID,
                (byte)(npcUniqueID >> 8),
                (byte)(npcUniqueID >> 16),
                (byte)(npcUniqueID >> 24)
            };
        }
    }
}
