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
            // all AdvancedPeds should call RegisterAdvancedPed but can't put it in the base constructor 
            // because the generic type will always be AdvancedPed, instead of Firefighter, Paramedic, etc. 
            // each AdvancedPed subclass with its own fiber and list
            RegisterAdvancedPed(this);
        }

        protected override void UpdateInternal()
        {
        }
    }
}
