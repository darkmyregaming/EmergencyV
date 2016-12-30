namespace EmergencyV
{
    // System
    using System;
    using System.Reflection;

    // RPH
    using Rage;

    internal class AIController
    {
        public bool IsEnabled { get; set; } = true;
        public Ped Ped { get; }

        private AITask currentTask;
        public AITask CurrentTask { get { return currentTask; } }

        public AIController(Ped ped)
        {
            Ped = ped;
        }

        public void Update()
        {
            if (IsEnabled && Ped && !Ped.IsDead)
            {
                if (currentTask != null)
                {
                    if (currentTask.IsFinished)
                    {
                        currentTask = null;
                        return;
                    }

                    currentTask?.Update();
                }
            }
        }

        public bool IsPerformingTask(AITask task)
        {
            return (currentTask != null && task != null && currentTask == task && !currentTask.IsFinished);
        }

        public bool IsPerformingTaskOfType<TTask>() where TTask : AITask
        {
            return (currentTask != null && currentTask.GetType() == typeof(TTask));
        }

        public AITask WalkTo(Vector3 position, float targetHeading, float distanceThreshold) => GiveTask<AITaskGoTo>(Ped, position, targetHeading, distanceThreshold, 1.0f);
        public AITask RunTo(Vector3 position, float targetHeading, float distanceThreshold) => GiveTask<AITaskGoTo>(Ped, position, targetHeading, distanceThreshold, 2.0f);
        public AITask WalkStraightTo(Vector3 position, float targetHeading, float distanceToSlideAt) => GiveTask<AITaskGoStraightTo>(Ped, position, targetHeading, distanceToSlideAt, 1.0f);
        public AITask RunStraightTo(Vector3 position, float targetHeading, float distanceToSlideAt) => GiveTask<AITaskGoStraightTo>(Ped, position, targetHeading, distanceToSlideAt, 2.0f);
        public AITask PerformCPR(Ped patient) => GiveTask<AITaskPerformCPR>(Ped, patient);

        protected AITask GiveTask<TTask>(params object[] args) where TTask : AITask
        {
            currentTask = (AITask)Activator.CreateInstance(typeof(TTask), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null,  args, null);
            Log($"GiveTask ({typeof(TTask).Name})");
            return currentTask;
        }

        private void Log(object o)
        {
            Game.LogTrivial($"[{this.GetType().Name} - CurrentTask:{(currentTask == null ? "None" : currentTask.GetType().Name)}] {o}");
        }
    }

    internal class AIFirefighterController : AIController
    {
        public Firefighter Firefighter { get; }

        public AIFirefighterController(Firefighter firefighter) : base(firefighter.Ped)
        {
            Firefighter = firefighter;
        }

        public AITask ExtinguishFireInArea(Vector3 position, float range) => GiveTask<AIFirefighterTaskExtinguishFireInArea>(Firefighter, position, range);
    }
}
