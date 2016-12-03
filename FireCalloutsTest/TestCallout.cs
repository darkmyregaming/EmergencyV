namespace FireCalloutsTest
{
    using System;
    using EmergencyV;

    [FireCalloutInfo("testing callout for engine", FirefighterRole.Engine, FireCalloutProbability.High)]
    [FireCalloutInfo("testing callout for rescue", FirefighterRole.Rescue, FireCalloutProbability.Medium)]
    internal class TestCallout : FireCallout
    {
        public override string DisplayName
        {
            get
            {
                return "test callout";
            }

            set
            {
            }
        }

        public override void ExecuteSomething()
        {
            Rage.Game.LogTrivial("Executing test callout");
            Rage.World.CleanWorld(true, true, true, true, true, true);
        }
    }
}
