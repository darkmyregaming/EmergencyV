namespace EmergencyV
{
    // RPH
    using Rage;

    internal class AITaskEnterVehicle : AITask
    {
        public Vehicle Vehicle { get; }

        Task goToTask;
        Task enterTask;
        int? index;
        EnterVehicleFlags flags;

        protected AITaskEnterVehicle(AIController controller, Vehicle vehicleToEnter, int? seatIndex /* if null, enter any free seat */, EnterVehicleFlags enterVehicleFlags) : base(controller)
        {
            Vehicle = vehicleToEnter;
            index = seatIndex;
            flags = enterVehicleFlags;
        }

        internal override void Update()
        {
            if (!Vehicle) // vehicle no longer exists, finish the task
            {
                IsFinished = true;
                return;
            }

            if (Vector3.DistanceSquared(Ped.Position, Vehicle) > 6.5f * 6.5f)
            {
                if ((goToTask == null || !goToTask.IsActive))
                    goToTask = Ped.Tasks.FollowNavigationMeshToPosition(Vehicle.Position, Ped.Position.GetHeadingTowards(Vehicle), 2.0f, 5.0f);
            }
            else if (!Ped.IsInVehicle(Vehicle, true) && (enterTask == null || !enterTask.IsActive))
            {
                int? i = index.HasValue ? index.Value : Vehicle.GetFreeSeatIndex();

                if (i.HasValue)
                {
                    enterTask = Ped.Tasks.EnterVehicle(Vehicle, i.Value, flags);
                }
                else
                {
                    IsFinished = true;
                }
            }
            else if (Ped.IsInVehicle(Vehicle, false))
            {
                IsFinished = true;
            }
        }

        protected override void OnFinished(bool aborted)
        {
            if (aborted)
                Ped.Tasks.Clear();
            goToTask = null;
            enterTask = null;
        }
    }
}
