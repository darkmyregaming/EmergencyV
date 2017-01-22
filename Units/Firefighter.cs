namespace EmergencyV
{
    // RPH
    using Rage;

    public class Firefighter : AdvancedPed
    {
        public FirefighterEquipmentController Equipment { get; }

        public Firefighter(Vector3 position, float heading)
             : base(Plugin.UserSettings.PEDS.FIREFIGHTER_MODEL, position, heading)
        {
            Equipment = new FirefighterEquipmentController(Ped);
        }
        
        protected override void UpdateInternal()
        {
            Equipment?.Update();
        }
    }
}
