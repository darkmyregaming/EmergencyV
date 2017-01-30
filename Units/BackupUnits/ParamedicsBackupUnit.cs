namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;

    // RPH
    using Rage;

    internal class ParamedicsBackupUnit : BackupUnit
    {
        public Paramedic[] Paramedics { get; private set; }

        public ParamedicsBackupUnit(Vector3 position, float heading) : base(position, heading, BlipSprite.ArmoredVan, "Paramedics Backup Unit", Color.FromArgb(33, 31, 96))
        {
            RegisterBackupUnit(this);
        }

        protected override void CreateInternal(out Vehicle vehicle, out AdvancedPed[] peds)
        {
            vehicle = EntityCreator.CreateEMSVehicle(SpawnLocation.Position, SpawnLocation.Heading);

            int seats = Math.Min(vehicle.PassengerCapacity + 1, 2);
            Paramedics = new Paramedic[seats];
            peds = new AdvancedPed[seats];
            for (int i = 0; i < seats; i++)
            {
                Paramedic p = new Paramedic(Vector3.Zero, 0.0f);
                p.PreferedVehicleSeatIndex = i - 1;
                p.Ped.WarpIntoVehicle(vehicle, i - 1);

                Paramedics[i] = p;
                peds[i] = p;
            }
        }

        protected override void UpdateInternal()
        {
        }

        protected override void DeleteInternal()
        {
            Paramedics = null;
        }

        protected override void DismissInternal()
        {
        }
    }
}
