namespace EmergencyV
{
    // RPH
    using Rage;

    /// <summary>
    /// Used for making an <see cref="AdvancedPed"/> execute complex tasks. 
    /// </summary>
    public abstract class AITask
    {
        public delegate void AITaskFinishedEventHandler(AITask task, bool aborted);

        public AIController Controller { get; }
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
                    OnFinished(false);
                    Finished?.Invoke(this, false);
                }
            }
        }

        public bool IsAborted { get; private set; }


        protected AITask(AIController controller)
        {
            Controller = controller;
            Ped = Controller.Owner.Ped;
        }

        public void Abort()
        {
            if (IsAborted)
                throw new System.InvalidOperationException($"The {this.GetType().Name} has already been aborted");
            if (IsFinished)
                throw new System.InvalidOperationException($"The {this.GetType().Name} has already been finished");

            IsAborted = true;
            isFinished = true;
            OnFinished(true);
            Finished?.Invoke(this, true);
        }

        internal abstract void Update();
        protected abstract void OnFinished(bool aborted);
    }
}
