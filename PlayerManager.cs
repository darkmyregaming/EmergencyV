namespace EmergencyV
{
    // System
    using System;
    using System.Linq;

    // RPH
    using Rage;
    using Rage.Native;

    public enum FirefighterRole
    {
        None = 0,
        Engine,
        Battalion,
        Rescue,
    }

    public enum PlayerStateType
    {
        Normal = 0,
        Firefighter = 1,
        EMS = 2,
    }

    internal class PlayerManager
    {
        private static PlayerManager instance;
        public static PlayerManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayerManager();
                return instance;
            }
        }

        private Model normalStateModel;

        private PlayerStateType playerState;
        public PlayerStateType PlayerState { get { return playerState; } }

        public bool IsEMS { get { return playerState == PlayerStateType.EMS; } }

        public bool IsFirefighter { get { return playerState == PlayerStateType.Firefighter; } }
        public FirefighterRole FirefighterRole { get; set; }

        private System.Collections.Generic.List<Firefighter> playerFirefighterPartners = new System.Collections.Generic.List<Firefighter>();

        private PlayerManager()
        {
            playerState = PlayerStateType.Normal;
            normalStateModel = Game.LocalPlayer.Model;
        }

        public void Update()
        {
        }

        public void SetPlayerToState(PlayerStateType type)
        {
            if (playerState == type)
                return;

            switch (type)
            {
                default:
                case PlayerStateType.Normal:
                    Game.MaxWantedLevel = 5;
                    Game.LocalPlayer.Model = normalStateModel;
                    FirefighterRole = FirefighterRole.None;
                    break;

                case PlayerStateType.Firefighter:
                    Game.MaxWantedLevel = 0;
                    normalStateModel = Game.LocalPlayer.Model;
                    Game.LocalPlayer.Model = Plugin.UserSettings.PEDS.FIREFIGHTER_MODEL;
                    FirefighterRole = FirefighterRole.Engine;
                    FireCalloutsManager.Instance.LoadCallouts();
                    FireCalloutsManager.Instance.ResetNextCalloutTimer();
                    break;
                case PlayerStateType.EMS:
                    Game.MaxWantedLevel = 0;
                    normalStateModel = Game.LocalPlayer.Model;
                    Game.LocalPlayer.Model = Plugin.UserSettings.PEDS.EMS_MODEL;
                    FirefighterRole = FirefighterRole.None;
                    EMSCalloutsManager.Instance.LoadCallouts();
                    EMSCalloutsManager.Instance.ResetNextCalloutTimer();
                    break;
            }

            API.Functions.OnPlayerStateChanged(type, playerState);
            playerState = type;
        }

        public void SpawnFirefighterPartner(Vector3 position) // TODO: this is just a placeholder for the partners
        {
            Firefighter f = new Firefighter(position, 0.0f);
            playerFirefighterPartners.Add(f);
            SetPartnersPreferedSeatIndex();

            f.AI.SetBehaviour<AIFirefighterPlayerPartnerBehaviour>();
        }

        private void SetPartnersPreferedSeatIndex()
        {
            for (int i = 0; i < playerFirefighterPartners.Count; i++)
            {
                playerFirefighterPartners[i].PreferedVehicleSeatIndex = i;
            }
        }
    }
}
