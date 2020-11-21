namespace RemoteController.Win32
{
    /// <summary>
    /// The AUTHENTICATION_REQUIREMENTS enumeration specifies the 'Man in the Middle' protection required for authentication.
    /// </summary>
    public enum BluetoothAuthenticationRequirements : int // MSFT+Win32 AUTHENTICATION_REQUIREMENTS
    {
        /// <summary>
        /// Protection against a "Man in the Middle" attack is not required for authentication.
        /// </summary>
        MITMProtectionNotRequired = 0,

        /// <summary>
        /// Protection against a "Man in the Middle" attack is required for authentication.
        /// </summary>
        MITMProtectionRequired = 0x1,

        /// <summary>
        /// Protection against a "Man in the Middle" attack is not required for bonding.
        /// </summary>
        MITMProtectionNotRequiredBonding = 0x2,

        /// <summary>
        /// Protection against a "Man in the Middle" attack is required for bonding.
        /// </summary>
        MITMProtectionRequiredBonding = 0x3,

        /// <summary>
        /// Protection against a "Man in the Middle" attack is not required for General Bonding.
        /// </summary>
        MITMProtectionNotRequiredGeneralBonding = 0x4,

        /// <summary>
        /// Protection against a "Man in the Middle" attack is required for General Bonding.
        /// </summary>
        MITMProtectionRequiredGeneralBonding = 0x5,

        /// <summary>
        /// Protection against "Man in the Middle" attack is not defined.
        /// </summary>
        MITMProtectionNotDefined = 0xff
    }
}