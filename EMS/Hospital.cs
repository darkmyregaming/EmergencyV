namespace EmergencyV
{
    using System.Drawing;

    using Rage;

    internal class Hospital : Building<HospitalData>
    {
        public Vehicle Ambulance { get; private set; }
        public RotatedVector3 AmbulanceSpawn { get; }

        public Vector3 DropOffLocation { get; }
        protected Blip DropOffBlip { get; }
        private bool IsDroppingOff { get; set; }

        protected override Color BlipColor { get { return Color.DarkRed; } }
        protected override string BlipName { get { return "Hospital"; } }
        protected override BlipSprite BlipSprite { get { return BlipSprite.Hospital; } }

        public Hospital(HospitalData data) : base(data)
        {
            Game.LogTrivialDebug("Loaded " + data.Name);
            AmbulanceSpawn = new RotatedVector3(data.AmbulanceSpawn.ToVector3(), new Rotator(0f, 0f, data.AmbulanceSpawn.W));

            DropOffLocation = data.DropOffLocation.ToVector3();

            if (DropOffLocation != Vector3.Zero) // some hospitals don't have a drop off
            {
                DropOffBlip = new Blip(DropOffLocation);
                DropOffBlip.Sprite = BlipSprite.Health;
                DropOffBlip.Color = Color.White;
                DropOffBlip.Name = "Emergency Room";
            }
        }

        protected override void UpdateInternal()
        {
            if (!IsDroppingOff && CanDropOff)
            {
                IsDroppingOff = true;
                GameFiber.StartNew(DropPatientOff);
            }
        }

        protected override void CreateInternal()
        {
            Ambulance = new Vehicle(Plugin.UserSettings.VEHICLES.AMBULANCE_MODEL, AmbulanceSpawn.Position, AmbulanceSpawn.Heading);
        }

        protected override void DeleteInternal()
        {
            if (Ambulance)
                Ambulance.Dismiss();
        }

        protected override void CleanUpInternal()
        {
            if (DropOffBlip)
                DropOffBlip.Delete();
        }

        private void DropPatientOff()
        {
            Vehicle current = Plugin.LocalPlayer.Character.CurrentVehicle;
            if (!current)
                return;

            Ped left = current.GetPedOnSeat(1);
            Ped right = current.GetPedOnSeat(2);
            if (!left && !right)
                return;

            int count = 0;

            Game.FadeScreenOut(1500, true);

            if (left)
            {
                left.Delete();
                count++;
            }

            if (right)
            {
                right.Delete();
                count++;
            }

            Game.FadeScreenIn(1500, true);

            Notification.Show(Name, $"{count} {(count > 1 ? "patients" : "patient")} have been checked in.", 4);

            IsDroppingOff = false;
        }

        private bool CanDropOff
        {
            get
            {
                Vehicle current = Plugin.LocalPlayer.Character.CurrentVehicle;
                if (current == null || current.Model.Name != Plugin.UserSettings.VEHICLES.AMBULANCE_MODEL || current.PassengerCount < 1)
                    return false;
                return Vector3.DistanceSquared(DropOffLocation, current.BelowPosition) <= 9.0f && current.Speed < 0.1f/*wait for the player to stop*/;
            }
        }
    }
}
