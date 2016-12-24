namespace EmergencyVDefaultCallouts
{
    // System
    using System;

    // EmergencyV
    using EmergencyV;
    using EmergencyV.API;

    // RPH
    using Rage;

    [EMSCalloutInfo("EMSCallTest", CalloutProbability.Medium)]
    internal class EMSCallTest : EMSCallout
    {
        public override bool OnBeforeCalloutDisplayed()
        {
            DisplayName = "Medical Emergency";
            DisplayExtraInfo = $"Unit: Paramedic\r\nLocation: {World.GetStreetName(Vector3.RandomUnit * MathHelper.GetRandomSingle(0.0f, 8000f))}\r\n";

            return base.OnBeforeCalloutDisplayed();
        }
    }
}
