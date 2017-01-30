namespace EmergencyV
{
    // System
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;

    internal class AIBackupUnitDriveToPositionTask : AIBackupUnitTask
    {
        public override AIBackupUnitTaskPriority Priority { get { return AIBackupUnitTaskPriority.High; } }

        Vector3 position;
        bool sirenOn;
        float speed;
        float acceptedDistance;
        VehicleDrivingFlags flags;

        List<AITask> enterVehicleTasks;
        AITask drivingTask;

        protected AIBackupUnitDriveToPositionTask(BackupUnit unit, Vector3 position, bool sirenOn, float speed, float acceptedDistance, VehicleDrivingFlags flags) : base(unit)
            {
            this.position = position;
            this.sirenOn = sirenOn;
            this.speed = speed;
            this.acceptedDistance = acceptedDistance;
            this.flags = flags;
        }

        protected override void StartInternal()
        {
            foreach (AdvancedPed a in Unit.Peds)
            {
                if (a.Ped)
                {
                    a.Ped.Tasks.Clear();
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
                Unit.Vehicle.IsSirenOn = sirenOn;
                drivingTask = Unit.Driver.AI.DriveTo(position, speed, acceptedDistance, flags);
            }
            else if (drivingTask != null && drivingTask.IsFinished)
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
