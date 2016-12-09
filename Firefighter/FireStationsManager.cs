namespace EmergencyV
{
    // RPH
    using Rage;

    internal class FireStationsManager
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

        public readonly FireStation[] FireStations;

        private FireStationRoleSelectionScreen roleSelectionScreen;

        private FireStationsManager()
        {
            FireStations = new FireStation[1]; // placeholder 
                                               // TODO: load fire stations from file
            FireStations[0] = new FireStation(FireStationDataPlaceHolder.Get());
            FireStations[0].PlayerEntered += OnPlayerEnteredFireStation;
        }

        public void Update()
        {
            bool shouldPlayerEnterStationIfNear = Game.IsControlJustPressed(0, GameControl.Context);
            Vector3 playerPos = Plugin.LocalPlayerCharacter.Position;
            for (int i = 0; i < FireStations.Length; i++)
            {
                FireStation station = FireStations[i];

                if (station.IsInActivationRangeFrom(playerPos))
                {
                    if (!station.IsCreated)
                        station.Create();
                }
                else if(station.IsCreated)
                {
                    station.Delete();
                }

                station.Update(shouldPlayerEnterStationIfNear);
            }

            roleSelectionScreen?.Update();
        }

        protected void OnPlayerEnteredFireStation(FireStation station)
        {
            Game.LogTrivial("Player Entered Fire Station: " + station.Data.Name);

            PlayerManager.Instance.SetPlayerToState(PlayerStateType.Firefighter);
            roleSelectionScreen = new FireStationRoleSelectionScreen(station);
            roleSelectionScreen.RoleSelected += OnFirefighterRoleSelected;
        }

        private void OnFirefighterRoleSelected(FirefighterRole role)
        {
            Game.LogTrivial("Player selected firefighter role: " + role);
            PlayerManager.Instance.FirefighterRole = role;
            FireCalloutsManager.Instance.LoadCallouts();
            FireCalloutsManager.Instance.ResetNextCalloutTimer();

            if (roleSelectionScreen != null)
            {
                Vehicle v = roleSelectionScreen.Station.GetVehicleForRole(role);
                if (v)
                {
                    Plugin.LocalPlayerCharacter.Position = v.FrontPosition + v.ForwardVector * 5.0f;
                    Plugin.LocalPlayerCharacter.Heading = MathHelper.ConvertDirectionToHeading((v.Position - Plugin.LocalPlayerCharacter.Position).ToNormalized());
                }

                roleSelectionScreen.CleanUp();
            }

            roleSelectionScreen = null;

        }

        public void CleanUp(bool isTerminating)
        {
            if (!isTerminating)
            {
                if (roleSelectionScreen != null)
                    roleSelectionScreen.CleanUp();

                for (int i = 0; i < FireStations.Length; i++)
                {
                    FireStation station = FireStations[i];
                    station.CleanUp();
                }
            }
        }
    }
}
