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
            for (int i = 0; i < FireStations.Length; i++)
            {
                FireStations[i].Update(shouldPlayerEnterStationIfNear);
            }
        }

        protected void OnPlayerEnteredFireStation(FireStation station)
        {
            Game.LogTrivial("Player Entered Fire Station: " + station.Data.Name);

            PlayerManager.Instance.SetPlayerToState(PlayerManager.PlayerStateType.FireFighter);
        }
    }
}
