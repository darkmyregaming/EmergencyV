namespace EmergencyV
{
    // RPH
    using Rage;

    public class Paramedic : AdvancedPed<AIParamedicController>
    {
        public Paramedic(Vector3 position, float heading)
             : base(Plugin.UserSettings.PEDS.EMS_MODEL, position, heading)
        {
        }

        protected override AIParamedicController CreateAIController() => new AIParamedicController(this);
    }
}
