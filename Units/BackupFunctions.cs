namespace EmergencyV
{
    // System
    using System.Linq;

    // RPH
    using Rage;
    using Rage.Native;

    internal static class BackupFunctions
    {
        internal enum FirefighterBackupTask
        {
            ExtinguishFireInArea,
        }

        internal enum ParamedicBackupTask
        {
            TODO,
        }

        public static void SendFirefightersUnit(Vector3 position, FirefighterBackupTask task)
        {
            RotatedVector3 spawn = Util.GetSpawnLocationAroundPlayer(true);

            FirefightersBackupUnit[] currentUnits = BackupUnit.GetAllBackupUnitsOfType<FirefightersBackupUnit>();

            FirefightersBackupUnit unit = (currentUnits == null || currentUnits.Length == 0) ?
                                                null :
                                                currentUnits.Where(u => !u.IsDismissedOrDeleted && !u.IsResponding).OrderBy(u => Vector3.DistanceSquared(u.Vehicle.Position, position)).FirstOrDefault();

            if (unit == null)
            {
                unit = new FirefightersBackupUnit(spawn.Position, spawn.Heading);
            }
            else
            {
                unit.AI.AbortAllTasks();
            }

            unit.AI.DriveToPosition(position, true, 15f, 20.0f, VehicleDrivingFlags.Emergency);
            unit.IsResponding = true;
            switch (task)
            {
                case FirefighterBackupTask.ExtinguishFireInArea:
                    unit.AI.ExtinguishFireInArea(position, 125.0f, false);
                    break;
            }

            AIBackupUnitTask unitTask = unit.AI.DriveToPosition(FireStationsManager.Instance.Buildings.OrderBy(s => Vector3.DistanceSquared(s.Entrance, spawn.Position)).First().Entrance, false, 8f, 50.0f, VehicleDrivingFlags.Normal, false);
            unitTask.Started += (t) =>
            {
                unit.IsResponding = false;
            };
            unitTask.Finished += (t, aborted) =>
            {
                if (!aborted)
                {
                    if (Vector3.DistanceSquared(Game.LocalPlayer.Character.Position, unit.Vehicle) > 70.0f * 70.0f)
                    {
                        unit.Delete();
                    }
                    else
                    {
                        unit.Dismiss();
                    }
                }
            };
        }

        public static void SendParamedicUnit(Vector3 position, ParamedicBackupTask task)
        {
            RotatedVector3 spawn = Util.GetSpawnLocationAroundPlayer(true);

            ParamedicsBackupUnit[] currentUnits = BackupUnit.GetAllBackupUnitsOfType<ParamedicsBackupUnit>();

            ParamedicsBackupUnit unit = (currentUnits == null || currentUnits.Length == 0) ?
                                                null :
                                                currentUnits.Where(u => !u.IsDismissedOrDeleted && !u.IsResponding).OrderBy(u => Vector3.DistanceSquared(u.Vehicle.Position, position)).FirstOrDefault();

            if (unit == null)
            {
                unit = new ParamedicsBackupUnit(spawn.Position, spawn.Heading);
            }
            else
            {
                unit.AI.AbortAllTasks();
            }

            unit.AI.DriveToPosition(position, true, 15f, 20.0f, VehicleDrivingFlags.Emergency);
            unit.IsResponding = true;
            switch (task)
            {
                default: break;
            }

            AIBackupUnitTask unitTask = unit.AI.DriveToPosition(HospitalsManager.Instance.Buildings.OrderBy(s => Vector3.DistanceSquared(s.Entrance, spawn.Position)).First().Entrance, false, 8f, 50.0f, VehicleDrivingFlags.Normal, false);
            unitTask.Started += (t) =>
            {
                unit.IsResponding = false;
            };
            unitTask.Finished += (t, aborted) =>
            {
                if (!aborted)
                {
                    if (Vector3.DistanceSquared(Game.LocalPlayer.Character.Position, unit.Vehicle) > 70.0f * 70.0f)
                    {
                        unit.Delete();
                    }
                    else
                    {
                        unit.Dismiss();
                    }
                }
            };
        }
    }
}
