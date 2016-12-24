namespace EmergencyV
{
    // System
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class EMSCalloutInfoAttribute : CalloutInfoAttribute
    {
        public EMSCalloutInfoAttribute(string calloutName, CalloutProbability probability) : base(calloutName, probability)
        {
        }

        internal override RegisteredCalloutData GetCalloutData(Type type)
        {
            return new EMSRegisteredCalloutData(type, CalloutName, Probability);
        }
    }
}
