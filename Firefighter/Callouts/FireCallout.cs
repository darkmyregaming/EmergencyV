namespace EmergencyV
{
    public abstract class FireCallout : Callout
    {
        /// <summary>
        /// Gets the player's current role.
        /// </summary>
        /// <value>
        /// The player's current role.
        /// </value>
        public FirefighterRole Role { get; internal set; }
    }
}
