namespace EmergencyV
{
    // System
    using System;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal static class Plugin
    {
        public static Player LocalPlayer;
        public static Ped LocalPlayerCharacter;

        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(500);

            while (true)
            {
                GameFiber.Yield();

                LocalPlayer = Game.LocalPlayer;
                LocalPlayerCharacter = LocalPlayer.Character;

                if (!Plugin.LocalPlayerCharacter)
                    continue;

                FireStationsManager.Instance.Update();
                PlayerManager.Instance.Update();
                PlayerEquipmentManager.Instance.Update();
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            if (!isTerminating)
            {
                // native calls
            }

            // dispose objects
        }
    }
}
