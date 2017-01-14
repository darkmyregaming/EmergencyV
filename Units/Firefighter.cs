namespace EmergencyV
{
    // RPH
    using Rage;

    public class Firefighter : AdvancedPed<AIFirefighterController>
    {
        public FirefighterEquipmentController Equipment { get; }

        public Firefighter(Vector3 position, float heading)
             : base(Plugin.UserSettings.PEDS.FIREFIGHTER_MODEL, position, heading)
        {
            Equipment = new FirefighterEquipmentController(Ped);
        }

        protected override AIFirefighterController CreateAIController() => new AIFirefighterController(this);

        protected override void Update()
        {
            base.Update();
            Equipment?.Update();
        }
    }
}
