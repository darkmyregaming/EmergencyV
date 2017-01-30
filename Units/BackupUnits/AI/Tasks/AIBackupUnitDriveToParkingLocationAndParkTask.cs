namespace EmergencyV
{
    // System
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class AIBackupUnitDriveToParkingLocationAndParkTask : AIBackupUnitTask
    {
        public override AIBackupUnitTaskPriority Priority { get { return AIBackupUnitTaskPriority.Medium; } }

        List<AITask> enterVehicleTasks;
        AITask drivingTask;
        Task parkTask;

        RotatedVector3 targetParkingLocation;

        protected AIBackupUnitDriveToParkingLocationAndParkTask(BackupUnit unit, RotatedVector3 parkingLocation) : base(unit)
        {
            targetParkingLocation = parkingLocation;
        }

        protected override void StartInternal()
        {
            foreach (AdvancedPed a in Unit.Peds)
            {
                a.Ped.Tasks.Clear();
                if (!a.Ped.IsInVehicle(Unit.Vehicle, false))
                {
                    if (enterVehicleTasks == null)
                        enterVehicleTasks = new List<AITask>();

                    enterVehicleTasks.Add(a.AI.EnterVehicle(Unit.Vehicle, a.PreferedVehicleSeatIndex));
                }
            }
        }

        protected override void UpdateInternal()
        {
            if (drivingTask == null && (enterVehicleTasks == null || enterVehicleTasks.All(t => t.IsFinished)))
            {
                Unit.Vehicle.IsSirenOn = false;
                Vector3 parkPos = targetParkingLocation.Position;
                Vector3 position;
                NativeFunction.Natives.GetClosestVehicleNode(parkPos.X, parkPos.Y, parkPos.Z, out position, 1, 3.0f, 0.0f);
                drivingTask = Unit.Driver.AI.DriveTo(position, 13.5f, 22.5f, VehicleDrivingFlags.YieldToCrossingPedestrians | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.DriveAroundPeds | VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.AllowWrongWay);
            }
            else if (parkTask == null && (drivingTask != null && drivingTask.IsFinished))
            {
                Vector3 parkPos = targetParkingLocation.Position;
                float parkHeading = targetParkingLocation.Heading;
                NativeFunction.Natives.TaskVehiclePark(Unit.Driver.Ped, Unit.Vehicle, parkPos.X, parkPos.Y, parkPos.Z, parkHeading, 1, 20f, false);
                parkTask = Task.GetTask(Unit.Driver.Ped, "TASK_VEHICLE_PARK");
            }
            else if (parkTask != null && !parkTask.IsActive)
            {
                IsFinished = true;
            }
        }

        protected override void OnFinished(bool aborted)
        {
            foreach (AdvancedPed a in Unit.Peds)
            {
                if (a.Ped) a.Ped.Tasks.Clear();
            }
        }
    }
}
