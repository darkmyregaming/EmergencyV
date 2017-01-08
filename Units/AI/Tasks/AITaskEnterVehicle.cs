namespace EmergencyV
{
    // RPH
    using Rage;

    class AITaskEnterVehicle : AITask
    {
        Task goToTask;
        Task enterTask;
        Vehicle vehicle;
        int? index;

        protected AITaskEnterVehicle(Ped ped, Vehicle vehicleToEnter, int? seatIndex /* if null, enter any free seat */) : base(ped)
        {
            vehicle = vehicleToEnter;
            index = seatIndex;
        }

        internal override void Update()
        {
            if (!vehicle) // vehicle no longer exists, finish the task
            {
                IsFinished = true;
                return;
            }

            if (Vector3.DistanceSquared(Ped.Position, vehicle) > 6.5f * 6.5f)
            {
                if ((goToTask == null || !goToTask.IsActive))
                    goToTask = Ped.Tasks.FollowNavigationMeshToPosition(vehicle.Position, Ped.Position.GetHeadingTowards(vehicle), 2.0f, 5.0f);
            }
            else if (!Ped.IsInVehicle(vehicle, true) && (enterTask == null || !enterTask.IsActive))
            {
                int? i = index.HasValue ? index.Value : vehicle.GetFreeSeatIndex();

                if (i.HasValue)
                {
                    enterTask = Ped.Tasks.EnterVehicle(vehicle, i.Value);
                }
                else
                {
                    IsFinished = true;
                }
            }
            else if (Ped.IsInVehicle(vehicle, false))
            {
                IsFinished = true;
            }
        }

        protected override void OnFinished()
        {
            goToTask = null;
            enterTask = null;
        }
    }
}
