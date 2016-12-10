namespace EmergencyVDefaultCallouts
{
    // System
    using System;

    // EmergencyV
    using EmergencyV;

    // RPH
    using Rage;

    [FireCalloutInfo("StructureFire", FirefighterRole.Engine, CalloutProbability.Medium)]
    internal class StructureFireCallout : FireCallout 
    {
        Blip blip;
        Fire[] fires;
        bool hasCreatedFires;

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("FROM CALLOUT: OnBeforeCalloutDisplayed()");

            DisplayName = "Structure Fire";
            DisplayExtraInfo = "Unit: Engine 42\r\nLocation: Somewhere in San Andreas";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Game.LogTrivial("FROM CALLOUT: OnCalloutAccepted()");

            blip = new Blip(new Vector3(150, -1037, 29));
            blip.IsRouteEnabled = true;

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            Game.LogTrivial("FROM CALLOUT: OnCalloutNotAccepted()");
            base.OnCalloutNotAccepted();
        }

        public override void Update()
        {
            if (!hasCreatedFires && Game.LocalPlayer.Character.DistanceTo2D(new Vector3(150, -1037, 29)) < 22.5f)
            {
                Game.DisplayNotification("creating fires");
                Vector3[] vectors = new Vector3[15];
                for (int i = 0; i < 15; i++)
                {
                    vectors[i] = new Vector3(150, -1037, 29).Around2D(0.5f, 6.25f);
                }

                fires = EmergencyV.API.Functions.CreateFires(vectors, 20, false, true);
                hasCreatedFires = true;
            }

            base.Update();
        }

        protected override void OnFinished()
        {
            Game.LogTrivial("FROM CALLOUT: OnFinished()");

            if (blip)
                blip.Delete();

            if (fires != null)
            {
                for (int i = 0; i < fires.Length; i++)
                {
                    if (fires[i])
                        fires[i].Delete();
                }
                fires = null;
            }

            base.OnFinished();
        }
    }
}
