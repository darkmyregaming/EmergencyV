namespace EmergencyV
{
    using System;
    // RPH
    using Rage;

    public class Paramedic : AdvancedPed
    {
        public Paramedic(Vector3 position, float heading)
             : base(Plugin.UserSettings.PEDS.EMS_MODEL, position, heading)
        {
        }

        protected override void UpdateInternal()
        {
        }
    }
}
