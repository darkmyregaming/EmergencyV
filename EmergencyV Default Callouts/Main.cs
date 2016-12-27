namespace EmergencyVDefaultCallouts
{
    using System;
    using Rage;
    using EmergencyV.API;

    internal class Main : Addon
    {
        public override string Name
        {
            get
            {
                return "Test Addon";
            }
        }

        public override void OnCleanUp()
        {
            Game.Console.Print("FROM ADDON: OnCleanUp()");
        }

        public override void OnStart()
        {
            Game.Console.Print("FROM ADDON: OnStart()");
            Functions.PlayerStateChanged += Functions_PlayerStateChanged;
        }

        private void Functions_PlayerStateChanged(EmergencyV.PlayerStateType currentState, EmergencyV.PlayerStateType previousState)
        {
            Game.Console.Print($"FROM ADDON: PlayerStateChanged-Currrent{currentState}-Previous{previousState}");
        }
    }
}
