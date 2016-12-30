namespace EmergencyV
{
    // RPH
    using Rage;

    internal class AITaskGoStraightTo : AITask
    {
        Task task;

        protected AITaskGoStraightTo(Ped ped, Vector3 position, float targetHeading, float distanceToSlideAt, float speed) : base(ped)
        {
            task = Ped.Tasks.GoStraightToPosition(position, speed, targetHeading, distanceToSlideAt, -1);
        }

        public override void Update()
        {
            if (task == null || !task.IsActive)
                IsFinished = true;
        }

        public override void OnFinished()
        {
            Ped.Tasks.Clear();
            task = null;
        }
    }
}
