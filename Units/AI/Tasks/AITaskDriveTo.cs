namespace EmergencyV
{
    // RPH
    using Rage;
    using Rage.Native;

    internal class AITaskDriveTo : AITask
    {
        Task task;

        protected AITaskDriveTo(AIController controller, Vector3 position, float speed, float acceptedDistance, VehicleDrivingFlags flags) : base(controller)
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
