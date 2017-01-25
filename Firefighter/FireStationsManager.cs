namespace EmergencyV
{
    using System;
    // RPH
    using Rage;

    internal class FireStationsManager : BuildingsManager<FireStation, FireStationData>
    {
        private static FireStationsManager instance;
        public static FireStationsManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new FireStationsManager();
                return instance;
            }
        }
        
        private FireStationRoleSelectionScreen roleSelectionScreen;

        private FireStationsManager() : base()
        {
        }

        public override void Update()
        {
            base.Update();

            roleSelectionScreen?.Update();
        }

        protected override void OnPlayerEnteredBuilding(FireStation station)
        {
            base.OnPlayerEnteredBuilding(station);
            
            PlayerManager.Instance.SetPlayerToState(PlayerStateType.Firefighter);
            roleSelectionScreen = new FireStationRoleSelectionScreen(station);
            roleSelectionScreen.RoleSelected += OnFirefighterRoleSelected;
            FireCalloutsManager.Instance.StartNewCalloutsAutomatically = false;
        }

        private void OnFirefighterRoleSelected(FirefighterRole role)
        {
            Game.LogTrivial("Player selected firefighter role: " + role);
            PlayerManager.Instance.FirefighterRole = role;

            if (roleSelectionScreen != null)
            {
                Vehicle v = roleSelectionScreen.Station.GetVehicleForRole(role);
                if (v)
                {
                    Game.LocalPlayer.Character.Position = v.FrontPosition + v.ForwardVector * 5.0f;
                    Game.LocalPlayer.Character.Heading = MathHelper.ConvertDirectionToHeading((v.Position - Game.LocalPlayer.Character.Position).ToNormalized());
                }

                roleSelectionScreen.CleanUp();
            }

            roleSelectionScreen = null;
            FireCalloutsManager.Instance.StartNewCalloutsAutomatically = true;
        }

        protected override FireStation[] LoadBuildings()
        {
            FireStationData[] data = FireStationData.GetDefaults();
            FireStation[] stations = new FireStation[data.Length];

            for (int i = 0; i < stations.Length; i++)
                stations[i] = new FireStation(data[i]);

            return stations;
        }

        public override void CleanUp(bool isTerminating)
        {
            if (roleSelectionScreen != null)
                roleSelectionScreen.CleanUp();

            base.CleanUp(isTerminating);
        }
    }
}
