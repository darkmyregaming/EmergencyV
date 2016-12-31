namespace EmergencyV
{
    // RPH
    using Rage;
    using Rage.Native;

    internal class AITaskDriveTo : AITask
    {
        Task task;

        protected AITaskDriveTo(Ped ped, Vector3 position, float speed, float acceptedDistance, VehicleDrivingFlags flags) : base(ped)
        {
            if (Ped.IsInAnyVehicle(true))
            {
                NativeFunction.Natives.TaskVehicleDriveToCoordLongrange(Ped, Ped.CurrentVehicle, position.X, position.Y, position.Z, speed, (uint)flags, acceptedDistance);
                task = Task.GetTask(Ped, "TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE");
            }
            else
            {
                IsFinished = true;
            }
        }

        public override void Update()
        {
            if (task == null || !task.IsActive)
                IsFinished = true;
        }

        public override void OnFinished()
        {
            task = null;
        }
    }
}
