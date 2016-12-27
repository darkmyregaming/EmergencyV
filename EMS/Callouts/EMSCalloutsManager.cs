namespace EmergencyV
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class EMSCalloutsManager : CalloutsManager<EMSRegisteredCalloutData, EMSCalloutInfoAttribute>
    {
        private static EMSCalloutsManager instance;
        public static EMSCalloutsManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new EMSCalloutsManager();
                return instance;
            }
        }
        
        public override bool CanUpdate { get { return PlayerManager.Instance.PlayerState == PlayerStateType.EMS && base.CanUpdate; } }

        public override IEnumerable<EMSRegisteredCalloutData> GetPossibleCallouts()
        {
            return RegisteredCalloutsData;
        }
    }
}
