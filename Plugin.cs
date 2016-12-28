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
        public const string AddonsFolder = ResourcesFolder + @"Addons\";

        public static readonly Random Random = new Random();

        private static ControlsSettings controls;
        public static ControlsSettings Controls { get { return controls; } }

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

            if (!Directory.Exists(AddonsFolder))
                Directory.CreateDirectory(AddonsFolder);

            LoadControls();
            LoadSettings();

            UIManager.Instance.Init();

            RespawnController.Instance.StartFiber();

            AddonsManager.Instance.LoadAddons();

            HoseTest hose = new HoseTest();
            while (true)
            {
                GameFiber.Yield();

                LocalPlayer = Game.LocalPlayer;
                LocalPlayerCharacter = LocalPlayer.Character;

                hose.Update();

                PlayerManager.Instance.Update();

                FireStationsManager.Instance.Update();
                HospitalsManager.Instance.Update();

                if (PlayerManager.Instance.IsFirefighter)
                {
                    FireCalloutsManager.Instance.Update();
                    PlayerFireEquipmentManager.Instance.Update();
                }
                else if (PlayerManager.Instance.IsEMS)
                {
                    EMSCalloutsManager.Instance.Update();
                    
                }

                CPR.Instance.Update();
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            AddonsManager.Instance.UnloadAddons();

            FireStationsManager.Instance.CleanUp(isTerminating);
            //PlayerManager.Instance.CleanUp(isTerminating);
            //PlayerEquipmentManager.Instance.CleanUp(isTerminating);
            FireCalloutsManager.Instance.CleanUp(isTerminating);

            HospitalsManager.Instance.CleanUp(isTerminating);
            EMSCalloutsManager.Instance.CleanUp(isTerminating);
        }

        private static void LoadControls()
        {
            controls = LoadFileFromResourcesFolder<ControlsSettings>("ControlsSettings.xml", ControlsSettings.GetDefault);
        }

        private static void LoadSettings()
        {
            userSettings = LoadFileFromResourcesFolder<Settings>("UserSettings.xml", Settings.GetDefault);
        }

        private static T LoadFileFromResourcesFolder<T>(string fileName, Func<T> getDefault)
        {
            string filePath = Path.Combine(ResourcesFolder, fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    Game.LogTrivial($"Deserializing {typeof(T).Name} from {fileName}");
                    return Util.Deserialize<T>(filePath);
                }
                catch (System.Runtime.Serialization.SerializationException ex)
                {
                    Game.LogTrivial($"Failed to deserilize {typeof(T).Name} from {fileName} - {ex}");
                }
            }

            Game.LogTrivial($"Loading {typeof(T).Name} default values and serializing to {fileName}");
            T defaults = getDefault();
            Util.Serialize(filePath, defaults);
            return defaults;
        }
    }
}
