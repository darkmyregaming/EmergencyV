namespace EmergencyV
{
    // RPH
    using Rage;

    internal class AITaskGoTo : AITask
    {
        Task task;

        protected AITaskGoTo(AIController controller, Vector3 position, float targetHeading, float distanceThreshold, float speed) : base(controller)
        {
            task = Ped.Tasks.FollowNavigationMeshToPosition(position, targetHeading, speed, distanceThreshold);
        }

        internal override void Update()
        {
            if (!Ped || Ped.IsDead)
            {
                Abort();
                return;
            }

            if (task == null || !task.IsActive)
                IsFinished = true;
        }

        protected override void OnFinished(bool aborted)
        {
            if (aborted)
                Ped.Tasks.Clear();
            task = null;
        }
    }

    internal class AITaskGoStraightTo : AITask
    {
        Task task;

        protected AITaskGoStraightTo(AIController controller, Vector3 position, float targetHeading, float distanceToSlideAt, float speed) : base(controller)
        {
            task = Ped.Tasks.GoStraightToPosition(position, speed, targetHeading, distanceToSlideAt, -1);
        }

        internal override void Update()
        {
            if (!Ped || Ped.IsDead)
            {
                Abort();
                return;
            }

            if (task == null || !task.IsActive)
                IsFinished = true;
        }

        protected override void OnFinished(bool aborted)
        {
            if (aborted)
                Ped.Tasks.Clear();
            task = null;
        }
    }
}
