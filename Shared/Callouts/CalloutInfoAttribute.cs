namespace EmergencyV
{
    // System
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public abstract class CalloutInfoAttribute : Attribute
    {
        public CalloutInfoAttribute(string calloutName, CalloutProbability probability)
        {
            CalloutName = calloutName;
            Probability = probability;
        }

        public string CalloutName { get; }
        public CalloutProbability Probability { get; }

        internal abstract RegisteredCalloutData GetCalloutData(Type calloutType);
    }
}
