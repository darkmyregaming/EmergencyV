namespace EmergencyV
{
    // System
    using System;

    internal class EMSRegisteredCalloutData : RegisteredCalloutData
    {
        public EMSRegisteredCalloutData(Type calloutType, string internalName, CalloutProbability probability) : base(calloutType, internalName, probability)
        {
        }
    }
}
