namespace EmergencyV
{
    // System
    using System;
    using System.IO;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal static class Plugin
    {
        public const string ResourcesFolder = @"Plugins\Emergency V\";

        public static readonly Random Random = new Random();

        private static Settings userSettings;
        public static Settings UserSettings { get { return userSettings; } }

        public static Player LocalPlayer;
        public static Ped LocalPlayerCharacter;

        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(500);

            MathHelper.RandomizationSeed = Environment.TickCount;

            if (!Directory.Exists(ResourcesFolder))
                Directory.CreateDirectory(ResourcesFolder);

            LoadSettings();

            HoseTest hose = new HoseTest();
            while (true)
            {
                GameFiber.Yield();

                LocalPlayer = Game.LocalPlayer;
                LocalPlayerCharacter = LocalPlayer.Character;

                hose.Update();

                if (Game.IsKeyDown(System.Windows.Forms.Keys.J))
                {
                    Vector3 p = Game.LocalPlayer.Character.GetOffsetPositionFront(5f);
                    p.Z = World.GetGroundZ(p, false, true).Value;

                    NativeFunction.Natives.StartScriptFire<uint>(p.X, p.Y, p.Z, 25, false);

                    p = Game.LocalPlayer.Character.GetOffsetPositionFront(7.5f);
                    p.Z = World.GetGroundZ(p, false, true).Value;
                    NativeFunction.Natives.StartScriptFire<uint>(p.X, p.Y, p.Z, 25, false);

                    p = Game.LocalPlayer.Character.GetOffsetPositionFront(9f);
                    p.Z = World.GetGroundZ(p, false, true).Value;
                    NativeFunction.Natives.StartScriptFire<uint>(p.X, p.Y, p.Z, 25, false);
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.U))
                {
                    Vehicle v = new Vehicle(Game.LocalPlayer.Character.GetOffsetPositionFront(10f));
                    v.Explode(true);
                    v.Dismiss();
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.I))
                {
                    FireCalloutsManager.Instance.LoadCallouts();
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.K))
                {
                    FireCalloutsManager.Instance.DoTest();
                }

                FireStationsManager.Instance.Update();
                PlayerManager.Instance.Update();
                PlayerFireEquipmentManager.Instance.Update();
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            FireStationsManager.Instance.CleanUp(isTerminating);
            //PlayerManager.Instance.CleanUp(isTerminating);
            //PlayerEquipmentManager.Instance.CleanUp(isTerminating);

            if (!isTerminating)
            {
                // native calls
            }

            // dispose objects
        }

        private static void LoadSettings()
        {
            string settingsFileName = ResourcesFolder + "UserSettings.xml";
            if (File.Exists(settingsFileName))
            {
                try
                {
                    Game.LogTrivial("Deserializing default settings from UserSettings.xml");
                    userSettings = Util.Deserialize<Settings>(settingsFileName);
                    return;
                }
                catch (System.Runtime.Serialization.SerializationException ex)
                {
                    Game.LogTrivial($"Failed to deserilize UserSettings.xml - {ex}");
                }
            }

            Game.LogTrivial("Loading default settings and serializing to UserSettings.xml");
            userSettings = Settings.GetDefault();
            Util.Serialize(settingsFileName, userSettings);
        }
    }
}
