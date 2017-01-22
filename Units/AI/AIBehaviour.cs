namespace EmergencyV
{
    // RPH
    using Rage;

    public abstract class AIBehaviour
    {
        public AIController Controller { get; }
        public Ped Ped { get; }

        protected AIBehaviour(AIController controller) // behaviours should only have a constructor with an AIController
        {
            Controller = controller;
            Ped = Controller.Owner.Ped;
        }

        internal abstract void Update();
    }
}
