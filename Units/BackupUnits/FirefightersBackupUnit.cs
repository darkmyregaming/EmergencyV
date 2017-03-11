namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;

    // RPH
    using Rage;

    internal class FirefightersBackupUnit : BackupUnit
    {
        public Firefighter[] Firefighters { get; private set; }

        public FirefightersBackupUnit(Vector3 position, float heading) : base(position, heading, BlipSprite.ArmoredVan, "Firefighters Backup Unit", Color.FromArgb(180, 0, 0))
        {
            RegisterBackupUnit(this);
        }

        protected override void CreateInternal(out Vehicle vehicle, out AdvancedPed[] peds)
        {
            vehicle = EntityCreator.CreateFirefighterVehicle(SpawnLocation.Position, SpawnLocation.Heading, FirefighterRole.Engine);

            int seats = Math.Min(vehicle.PassengerCapacity + 1, 4);
            Firefighters = new Firefighter[seats];
            peds = new AdvancedPed[seats];
            for (int i = 0; i < seats; i++)
            {
                Firefighter f = new Firefighter(Vector3.Zero, 0.0f);
                f.PreferedVehicleSeatIndex = i - 1;
                f.Ped.WarpIntoVehicle(vehicle, i - 1);
                f.Equipment.SetEquipped<FireGearEquipment>(false);

                Firefighters[i] = f;
                peds[i] = f;
            }
        }

        protected override void UpdateInternal()
        {
        }

        protected override void DeleteInternal()
        {
            Firefighters = null;
        }

        protected override void DismissInternal()
        {
        }
    }
}
