namespace EmergencyV
{
    // RPH
    using Rage;

    internal class AITaskLeaveVehicle : AITask
    {
        Task task;

        protected AITaskLeaveVehicle(AIController controller, LeaveVehicleFlags flags) : base(controller)
        {
            if (Ped.IsInAnyVehicle(true))
            {
                task = Ped.Tasks.LeaveVehicle(Ped.CurrentVehicle, flags);
            }
            else
            {
                IsFinished = true;
            }
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
