namespace EmergencyV
{
    internal class HospitalsManager : BuildingsManager<Hospital, HospitalData>
    {
        private static HospitalsManager instance;
        public static HospitalsManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new HospitalsManager();
                return instance;
            }
        }

        protected override void OnPlayerEnteredBuilding(Hospital h)
        {
            base.OnPlayerEnteredBuilding(h);

            PlayerManager.Instance.SetPlayerToState(PlayerStateType.EMS);
            if (h.Ambulance)
            {
                int? seat = h.Ambulance.GetFreeSeatIndex();
                if (seat.HasValue)
                    Plugin.LocalPlayer.Character.WarpIntoVehicle(h.Ambulance, seat.Value);
            }

            EMSCalloutsManager.Instance.LoadCallouts();
        }

        protected override Hospital[] LoadBuildings()
        {
            HospitalData[] data = HospitalData.GetDefaults();
            Hospital[] hospitals = new Hospital[data.Length];

            for (int i = 0; i < hospitals.Length; i++)
                hospitals[i] = new Hospital(data[i]);

            return hospitals;
        }

        public override void CleanUp(bool isTerminating)
        {
            base.CleanUp(isTerminating);
        }
    }
}