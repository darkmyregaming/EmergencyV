namespace EmergencyV
{
    // RPH
    using Rage;

    public abstract class AITask
    {
        public delegate void AITaskFinishedEventHandler(AITask task);

        public Ped Ped { get; }
        public event AITaskFinishedEventHandler Finished;
        private bool isFinished;
        public bool IsFinished
        {
            get { return isFinished; }
            protected set
            {
                if (value == isFinished)
                    return;
                isFinished = value;
                if (isFinished)
                {
                    OnFinished();
                    Finished?.Invoke(this);
                }
            }
        }


        protected AITask(Ped ped)
        {
            Ped = ped;
        }

        internal abstract void Update();
        protected abstract void OnFinished();
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
