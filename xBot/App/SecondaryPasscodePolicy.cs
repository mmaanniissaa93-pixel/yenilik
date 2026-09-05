using System;

namespace xBot.App
{
    /// <summary>
    /// Pure decision engine for Silkroad secondary passcode (PIN) handling.
    /// Free of UI or network dependencies for complete unit testability.
    /// </summary>
    public static class SecondaryPasscodePolicy
    {
        public const ushort CLIENT_SECONDARY_PASSCODE = 0x7625;
        public const ushort SERVER_SECONDARY_PASSCODE_REQUEST = 0x3625;
        public const ushort SERVER_SECONDARY_PASSCODE_RESPONSE = 0xB625;

        /// <summary>
        /// Validates whether the given PIN meets Silkroad secondary passcode criteria (typically 4 to 8 characters).
        /// </summary>
        public static bool IsValidPasscode(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
                return false;

            string trimmed = pin.Trim();
            return trimmed.Length >= 4 && trimmed.Length <= 8;
        }

        /// <summary>
        /// Resolves the active PIN by prioritizing the account-specific passcode,
        /// falling back to the global passcode if the account has none.
        /// </summary>
        public static string ResolvePasscode(string accountPin, string globalPin)
        {
            if (IsValidPasscode(accountPin))
                return accountPin.Trim();

            if (IsValidPasscode(globalPin))
                return globalPin.Trim();

            return string.Empty;
        }

        /// <summary>
        /// Checks if an opcode represents a secondary passcode request or response.
        /// </summary>
        public static bool IsPasscodeOpcode(ushort opcode)
        {
            return opcode == CLIENT_SECONDARY_PASSCODE ||
                   opcode == SERVER_SECONDARY_PASSCODE_REQUEST ||
                   opcode == SERVER_SECONDARY_PASSCODE_RESPONSE;
        }

        /// <summary>
        /// Determines whether the bot should automatically inject the secondary passcode.
        /// </summary>
        public static bool ShouldSendPasscode(bool autoEnterEnabled, string resolvedPin)
        {
            return autoEnterEnabled && IsValidPasscode(resolvedPin);
        }

        /// <summary>
        /// Determines whether the bot should send secondary passcode for an account.
        /// If the account has a valid saved passcode, it is automatically sent.
        /// Otherwise falls back to global auto-enter settings.
        /// </summary>
        public static bool ShouldSendPasscodeForAccount(string accountPin, bool globalAutoEnter, string globalPin)
        {
            if (IsValidPasscode(accountPin))
                return true;

            if (globalAutoEnter && IsValidPasscode(globalPin))
                return true;

            return false;
        }
    }
}
