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

            // all AdvancedPeds should call RegisterAdvancedPed but can't put it in the base constructor 
            // because the generic type will always be AdvancedPed, instead of Firefighter, Paramedic, etc. 
            // each AdvancedPed subclass with its own fiber and list
            RegisterAdvancedPed(this); 
        }
        
        protected override void UpdateInternal()
        {
            Equipment?.Update();
        }
    }
}
