namespace EmergencyV
{
    // RPH
    using Rage;

    internal class AITaskLeaveVehicle : AITask
    {
        Task task;

        protected AITaskLeaveVehicle(Ped ped, LeaveVehicleFlags flags) : base(ped)
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
            if (task == null || !task.IsActive)
                IsFinished = true;
        }

        protected override void OnFinished()
        {
            task = null;
        }
    }
}
