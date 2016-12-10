namespace EmergencyV
{
    // System
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class FireCalloutInfoAttribute : CalloutInfoAttribute
    {
        public FireCalloutInfoAttribute(string calloutName, FirefighterRole role, CalloutProbability probability) : base(calloutName, probability)
        {
            Role = role;
        }
        
        public FirefighterRole Role { get; }

        internal override RegisteredCalloutData GetCalloutData(Type type)
        {
            return new FireRegisteredCalloutData(type, CalloutName, Role, Probability);
        }
    }
}
