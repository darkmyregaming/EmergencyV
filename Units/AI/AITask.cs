namespace EmergencyV
{
    // RPH
    using Rage;

    public abstract class AITask
    {
        public Ped Ped { get; }
        private bool finished;
        public bool IsFinished
        {
            get { return finished; }
            protected set
            {
                if (value == finished)
                    return;
                finished = value;
                if (finished)
                    OnFinished();
            }
        }

        protected AITask(Ped ped)
        {
            Ped = ped;
        }

        public abstract void Update();
        public abstract void OnFinished();
    }

    public abstract class AIFirefighterTask : AITask
    {
        public Firefighter Firefighter { get; }

        protected AIFirefighterTask(Firefighter firefighter) : base(firefighter.Ped)
        {
            Firefighter = firefighter;
        }
    }
}
