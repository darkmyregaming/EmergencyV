namespace EmergencyV
{
    // System
    using System;

    internal abstract class RegisteredCalloutData
    {
        public Type CalloutType { get; }
        public string InternalName { get; }
        public CalloutProbability Probability { get; }

        public RegisteredCalloutData(Type calloutType, string internalName, CalloutProbability probability)
        {
            CalloutType = calloutType;
            InternalName = internalName;
            Probability = probability;
        }
    }
}
