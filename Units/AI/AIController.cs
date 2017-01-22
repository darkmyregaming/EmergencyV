namespace EmergencyV
{
    // System
    using System;
    using System.Reflection;

    // RPH
    using Rage;

    public class AIController
    {
        public bool IsEnabled { get; set; } = true;
        public AdvancedPed Owner { get; }

        private AITask currentTask;
        public AITask CurrentTask { get { return currentTask; } }

        private AIBehaviour behaviour;
        public AIBehaviour Behaviour { get { return behaviour; } }

        internal AIController(AdvancedPed owner)
        {
            Owner = owner;
        }

        internal void Update()
        {
            if (IsEnabled && Owner.Ped && !Owner.Ped.IsDead)
            {
                if (currentTask != null)
                {
                    if (currentTask.IsFinished)
                    {
                        currentTask = null;
                        return;
                    }

                    currentTask.Update();
                }

                behaviour?.Update();
            }
        }

        public bool IsPerformingAnyTask()
        {
            return (currentTask != null && !currentTask.IsFinished);
        }

        public bool IsPerformingTask(AITask task)
        {
            return (currentTask != null && task != null && currentTask == task && !currentTask.IsFinished);
        }

        public bool IsPerformingTaskOfType<TTask>() where TTask : AITask
        {
            return (currentTask != null && currentTask.GetType() == typeof(TTask));
        }

        public AITask WalkTo(Vector3 position, float targetHeading, float distanceThreshold) => GiveTask<AITaskGoTo>(this, position, targetHeading, distanceThreshold, 1.0f);
        public AITask RunTo(Vector3 position, float targetHeading, float distanceThreshold) => GiveTask<AITaskGoTo>(this, position, targetHeading, distanceThreshold, 2.0f);
        public AITask WalkStraightTo(Vector3 position, float targetHeading, float distanceToSlideAt) => GiveTask<AITaskGoStraightTo>(this, position, targetHeading, distanceToSlideAt, 1.0f);
        public AITask RunStraightTo(Vector3 position, float targetHeading, float distanceToSlideAt) => GiveTask<AITaskGoStraightTo>(this, position, targetHeading, distanceToSlideAt, 2.0f);
        public AITask PerformCPR(Ped patient) => GiveTask<AITaskPerformCPR>(this, patient);
        public AITask EnterVehicle(Vehicle vehicleToEnter, int? seatIndex, EnterVehicleFlags flags = EnterVehicleFlags.None) => GiveTask<AITaskEnterVehicle>(this, vehicleToEnter, seatIndex, flags);
        public AITask LeaveVehicle(LeaveVehicleFlags flags) => GiveTask<AITaskLeaveVehicle>(this, flags);
        public AITask DriveTo(Vector3 position, float speed, float acceptedDistance, VehicleDrivingFlags flags) => GiveTask<AITaskDriveTo>(this, position, speed, acceptedDistance, flags);
        public AITask ExtinguishFireInArea(Vector3 position, float range, bool shouldUseVehicleWaterCannon = true) => GiveTask<AITaskExtinguishFireInArea>(this, position, range, shouldUseVehicleWaterCannon);
        public AITask Follow(Entity entityToFollow, Vector3 offset, float stoppingRange, float speed, bool persistFollowing = true) => GiveTask<AITaskFollow>(this, entityToFollow, offset, stoppingRange, speed, persistFollowing);

        protected AITask GiveTask<TTask>(params object[] args) where TTask : AITask
        {
            currentTask = (AITask)Activator.CreateInstance(typeof(TTask), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null,  args, null);
            Log($"GiveTask ({typeof(TTask).Name})");
            return currentTask;
        }

        public AIBehaviour SetBehaviour<TBehaviour>() where TBehaviour : AIBehaviour
        {
            behaviour = (AIBehaviour)Activator.CreateInstance(typeof(TBehaviour), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { this }, null);
            Log($"SetBehaviour ({typeof(TBehaviour).Name})");
            return behaviour;
        }

        private void Log(object o)
        {
            Game.LogTrivial($"[{this.GetType().Name}<{Owner.GetType().Name}>] {o}");
        }
    }
}
