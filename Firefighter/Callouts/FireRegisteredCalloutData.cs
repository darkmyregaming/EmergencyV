namespace EmergencyV
{
    // System
    using System;

    internal class FireRegisteredCalloutData : RegisteredCalloutData
    {
        public FirefighterRole Role { get; }

        public FireRegisteredCalloutData(Type calloutType, string internalName, FirefighterRole role, CalloutProbability probability) : base(calloutType, internalName, probability)
        {
            Role = role;
        }
    }
}
