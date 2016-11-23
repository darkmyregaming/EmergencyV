namespace EmergencyV
{
    // System
    using System;
    using System.Linq;

    // RPH
    using Rage;
    using Rage.Native;

    internal class PlayerManager
    {
        public enum PlayerStateType
        {
            Normal = 0,
            FireFighter = 1,
        }

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

        public bool IsFireFighter { get { return playerState == PlayerStateType.FireFighter; } }

        private PlayerManager()
        {
            playerState = PlayerStateType.Normal;
            normalStateModel = Plugin.LocalPlayer.Model;
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
                    Plugin.LocalPlayer.Model = normalStateModel;
                    break;

                case PlayerStateType.FireFighter:
                    normalStateModel = Plugin.LocalPlayer.Model;
                    Plugin.LocalPlayer.Model = "S_M_Y_FIREMAN_01";
                    // Plugin.LocalPlayerCharacter.SetVariation();
                    break;
            }

            playerState = type;
        }
    }
}
