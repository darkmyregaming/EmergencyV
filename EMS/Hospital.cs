namespace EmergencyV
{
    using System.Drawing;

    using Rage;
    
    internal class Hospital : Building<HospitalData>
    {
        public Vehicle Ambulance { get; private set; }
        public RotatedVector3 AmbulanceSpawn { get; }

        protected override Color BlipColor { get { return Color.DarkRed; } }
        protected override string BlipName { get { return "Hospital"; } }
        protected override BlipSprite BlipSprite { get { return BlipSprite.Hospital; } }

        public Hospital(HospitalData data) : base(data)
        {
            Game.LogTrivialDebug("Loaded " + data.Name);
            AmbulanceSpawn = new RotatedVector3(data.AmbulanceSpawn.ToVector3(), new Rotator(0f, 0f, data.AmbulanceSpawn.W));
        }

        protected override void CreateInternal()
        {
            Ambulance = new Vehicle("AMBULANCE", AmbulanceSpawn.Position, AmbulanceSpawn.Heading);
        }

        protected override void DeleteInternal()
        {
            if (Ambulance)
                Ambulance.Dismiss();
        }
    }
}
