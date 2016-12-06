namespace EmergencyV
{
    // System
    using System;
    using System.IO;
    using System.Linq;
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

            UIManager.Instance.Init();

            HoseTest hose = new HoseTest();
            while (true)
            {
                GameFiber.Yield();

                LocalPlayer = Game.LocalPlayer;
                LocalPlayerCharacter = LocalPlayer.Character;

                hose.Update();

                if (Game.IsKeyDown(System.Windows.Forms.Keys.J))
                {
                    List<Vector3> positions = new List<Vector3>();
                    for (float i = 5f; i < 15f; i += 2.0f)
                    {
                        positions.Add(Plugin.LocalPlayerCharacter.GetOffsetPositionFront(i));
                    }

                    Game.LogTrivial(positions.Count + " positions collected");

                    Fire[] fires = Util.CreateFires(positions.ToArray(), 25, false);

                    Game.LogTrivial(fires.Length + " fires found");

                    GameFiber.StartNew(() =>
                    {
                        GameFiber.Sleep(30000);
                        for (int i = 0; i < fires.Length; i++)
                        {
                            if (fires[i])
                                fires[i].Delete();
                        }
                    });
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
                    Notification.ShowTest();
                }

                FireStationsManager.Instance.Update();
                PlayerManager.Instance.Update();
                PlayerFireEquipmentManager.Instance.Update();
                FireCalloutsManager.Instance.Update();
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            FireStationsManager.Instance.CleanUp(isTerminating);
            //PlayerManager.Instance.CleanUp(isTerminating);
            //PlayerEquipmentManager.Instance.CleanUp(isTerminating);
            FireCalloutsManager.Instance.CleanUp(isTerminating);

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
                    Game.LogTrivial("Deserializing settings from UserSettings.xml");
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
