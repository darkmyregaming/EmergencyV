namespace EmergencyVDefaultCallouts
{
    using System;
    using Rage;
    using EmergencyV;
    using EmergencyV.API;

    internal class Main : Addon
    {
        private bool Running { get; set; } = false;

        public override void OnCleanUp()
        {
            Game.Console.Print("FROM ADDON: OnCleanUp()");
            Running = false;
        }

        public override void OnStart()
        {
            Game.Console.Print("FROM ADDON: OnStart()");

            Control? printKey = Functions.GetOrRegisterControlMapping("ADDON_PRINT", new Control(System.Windows.Forms.Keys.T, ControllerButtons.None));
            if (!printKey.HasValue)
                throw new Exception("Failed to map key for addon");

            Control? foobarKey = Functions.GetOrRegisterControlMapping("ADDON_FOOBAR", new Control(System.Windows.Forms.Keys.O, ControllerButtons.None));
            if (!foobarKey.HasValue)
                throw new Exception("Failed to map key for addon");

            Control? fooKey = Functions.GetOrRegisterControlMapping("ADDON_FOO", new Control(System.Windows.Forms.Keys.O, ControllerButtons.None));
            if (fooKey.HasValue)
                throw new Exception("Shouldn't have registered control mapping");

            Functions.RemoveControlMapping("ADDON_FOO");

            GameFiber.StartNew(() =>
            {
                Running = true;
                while (Running)
                {
                    if (printKey.Value.IsJustPressed())
                        Game.LogTrivial("Pressed print key");

                    GameFiber.Yield();
                }
            });

            GameFiber.StartNew(() =>
            {
                GameFiber.Sleep(5000);
                Functions.RemoveControlMapping("ADDON_FOOBAR");
            });

            Functions.PlayerStateChanged += Functions_PlayerStateChanged;
            Functions.RegisteringFirefighterCallouts += Functions_RegisteringFirefighterCallouts;
            Functions.RegisteringEMSCallouts += Functions_RegisteringEMSCallouts;
        }

        private void Functions_RegisteringEMSCallouts()
        {
            Game.Console.Print($"FROM ADDON: RegisteringEMSCallouts");
        }

        private void Functions_RegisteringFirefighterCallouts()
        {
            Game.Console.Print($"FROM ADDON: RegisteringFirefighterCallouts");
        }

        private void Functions_PlayerStateChanged(EmergencyV.PlayerStateType currentState, EmergencyV.PlayerStateType previousState)
        {
            Game.Console.Print($"FROM ADDON: PlayerStateChanged-Currrent{currentState}-Previous{previousState}");
        }
    }
}
