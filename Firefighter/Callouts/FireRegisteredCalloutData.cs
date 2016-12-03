namespace EmergencyV
{
    // System
    using System;

    internal struct FireRegisteredCalloutData
    {
        public Type CalloutType { get; }
        public string InternalName { get; }
        public FirefighterRole Role { get; }
        public FireCalloutProbability Probability { get; }

        public FireRegisteredCalloutData(Type calloutType, string internalName, FirefighterRole role, FireCalloutProbability probability)
        {
            CalloutType = calloutType;
            InternalName = internalName;
            Role = role;
            Probability = probability;            
        }
    }
}
