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

        // returns true if a unit was sent, otherwise false
        public static bool SendFirefightersUnit(Vector3 position, FirefighterBackupTask task)
        {
            FireStation closestFireStationWithUnits = FireStationsManager.Instance.Buildings.FirstOrDefault(s => s.Units.Length > 0 && s.Units.Any(u => u.IsParkedAtFireStation));
            if (closestFireStationWithUnits != null)
            {
                FirefightersUnit unit = MathHelper.Choose(closestFireStationWithUnits.Units);
                unit.AI.DriveToPosition(Game.LocalPlayer.Character.Position, true, 15f, 20.0f, VehicleDrivingFlags.Emergency);
                switch (task)
                {
                    case FirefighterBackupTask.ExtinguishFireInArea:
                        unit.AI.ExtinguishFireInArea(Game.LocalPlayer.Character.Position, 100.0f, false);
                        break;
                }
                unit.AI.DriveToStationAndPark(false);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
