namespace EmergencyV
{
    // RPH
    using Rage;

    internal class AITaskGoTo : AITask
    {
        Task task;

        protected AITaskGoTo(Ped ped, Vector3 position, float targetHeading, float distanceThreshold, float speed) : base(ped)
        {
            task = Ped.Tasks.FollowNavigationMeshToPosition(position, targetHeading, speed, distanceThreshold);
        }

        internal override void Update()
        {
            if (task == null || !task.IsActive)
                IsFinished = true;
        }

        protected override void OnFinished()
        {
            task = null;
        }
    }
}
