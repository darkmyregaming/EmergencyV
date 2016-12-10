namespace EmergencyV
{
    // System
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;

    // RPH
    using Rage;

    internal class FireCalloutsManager : CalloutsManager<FireCallout, FireRegisteredCalloutData, FireCalloutInfoAttribute>
    {
        private static FireCalloutsManager instance;
        public static FireCalloutsManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new FireCalloutsManager();
                return instance;
            }
        }

        public override string CalloutsFolder { get { return Plugin.ResourcesFolder + @"Fire Callouts\"; } }

        public override bool CanUpdate { get { return PlayerManager.Instance.IsFirefighter && base.CanUpdate; } }

        public override IEnumerable<FireRegisteredCalloutData> GetPossibleCallouts() // if Role == None all callouts can be returned
        {
            return PlayerManager.Instance.FirefighterRole == FirefighterRole.None ? RegisteredCalloutsData : RegisteredCalloutsData.Where(d => d.Role == PlayerManager.Instance.FirefighterRole);
        }

        protected override void OnCalloutCreated(FireCallout callout)
        {
            base.OnCalloutCreated(callout);

            callout.Role = PlayerManager.Instance.FirefighterRole;
        }
    }
}
