namespace EmergencyV
{
    // System
    using System;

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
