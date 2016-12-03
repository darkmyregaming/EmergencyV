namespace EmergencyV
{
    // System
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class FireCalloutInfoAttribute : Attribute
    {
        public FireCalloutInfoAttribute(string calloutName, FirefighterRole role, FireCalloutProbability probability)
        {
            CalloutName = calloutName;
            Role = role;
            Probability = probability;
        }

        public string CalloutName { get; }
        public FirefighterRole Role { get; }
        public FireCalloutProbability Probability { get; }
    }
}
