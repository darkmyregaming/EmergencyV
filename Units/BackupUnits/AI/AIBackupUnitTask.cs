namespace EmergencyV
{
    // System
    using System;

    internal enum AIBackupUnitTaskPriority
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
    }

    internal abstract class AIBackupUnitTask
    {
        public abstract AIBackupUnitTaskPriority Priority { get; }

        public delegate void AIBackupUnitTaskStartedEventHandler(AIBackupUnitTask task);
        public delegate void AIBackupUnitTaskFinishedEventHandler(AIBackupUnitTask task, bool aborted);

        public BackupUnit Unit { get; }
        public event AIBackupUnitTaskStartedEventHandler Started;
        public event AIBackupUnitTaskFinishedEventHandler Finished;

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

        public bool HasStarted { get; private set; }

        protected AIBackupUnitTask(BackupUnit unit)
        {
            Unit = unit;
        }

        internal void Start()
        {
            if (HasStarted)
                throw new InvalidOperationException($"The {this.GetType().Name} has already been started");

            StartInternal();

            HasStarted = true;

            Started?.Invoke(this);
        }

        internal void Update()
        {
            if (HasStarted && !IsAborted && !IsFinished)
                UpdateInternal();
        }

        internal void Abort()
        {
            if (IsAborted)
                throw new InvalidOperationException($"The {this.GetType().Name} has already been aborted");
            if (IsFinished)
                throw new InvalidOperationException($"The {this.GetType().Name} has already been finished");

            IsAborted = true;
            isFinished = true;
            OnFinished(true);
            Finished?.Invoke(this, true);
        }

        protected abstract void StartInternal();
        protected abstract void UpdateInternal();
        protected abstract void OnFinished(bool aborted);
    }
}
