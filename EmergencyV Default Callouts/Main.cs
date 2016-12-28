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
            Functions.RegisteringFirefighterCallouts += Functions_RegisteringFirefighterCallouts;
            Functions.RegisteringEMSCallouts += Functions_RegisteringEMSCallouts;
        }

        private void Functions_RegisteringEMSCallouts()
        {
            Game.Console.Print($"FROM ADDON: RegisteringEMSCallouts");
            Functions.RegisterEMSCallout(typeof(callout), "CALLOUT FROM ADDON", EmergencyV.CalloutProbability.VeryHigh);
        }

        private void Functions_RegisteringFirefighterCallouts()
        {
            Game.Console.Print($"FROM ADDON: RegisteringFirefighterCallouts");
            Functions.RegisterFirefighterCallout(typeof(callout), "CALLOUT FROM ADDON", EmergencyV.FirefighterRole.Engine, EmergencyV.CalloutProbability.VeryHigh);
        }

        private void Functions_PlayerStateChanged(EmergencyV.PlayerStateType currentState, EmergencyV.PlayerStateType previousState)
        {
            Game.Console.Print($"FROM ADDON: PlayerStateChanged-Currrent{currentState}-Previous{previousState}");
        }
    }


    internal class callout : EmergencyV.Callout
    {
    }
}
