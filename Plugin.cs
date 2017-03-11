namespace EmergencyV
{
    // System
    using System;
    using System.IO;

    // RPH
    using Rage;
    using Rage.Native;

    internal static class Plugin
    {
        public const string ResourcesFolder = @"Plugins\Emergency V\";
        public const string AddonsFolder = ResourcesFolder + @"Addons\";

        public static readonly Random Random = new Random();
        
        public static Controls Controls { get; private set; }
        public static Settings UserSettings { get; private set; }

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

            NotificationsManager.Instance.StartFiber();
            RespawnController.Instance.StartFiber();

            AddonsManager.Instance.LoadAddons();

            FirefighterEquipmentController.RegisterEquipments();

            while (true)
            {
                GameFiber.Yield();
                
                PluginMenu.Instance.Update();

                PlayerManager.Instance.Update();

                FireStationsManager.Instance.Update();
                HospitalsManager.Instance.Update();

                if (PlayerManager.Instance.IsFirefighter)
                {
                    FireCalloutsManager.Instance.Update();
                    PlayerFireEquipmentController.Instance.Update();
                }
                else if (PlayerManager.Instance.IsEMS)
                {
                    EMSCalloutsManager.Instance.Update();
                    
                }

                CPRManager.Instance.Update();
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            AddonsManager.Instance.UnloadAddons();

            FireStationsManager.Instance.CleanUp(isTerminating);
            //PlayerManager.Instance.CleanUp(isTerminating);
            PlayerFireEquipmentController.Instance.CleanUp(isTerminating);
            FireCalloutsManager.Instance.CleanUp(isTerminating);

            HospitalsManager.Instance.CleanUp(isTerminating);
            EMSCalloutsManager.Instance.CleanUp(isTerminating);

            FirefightersBackupUnit[] firefightersUnits = BackupUnit.GetAllBackupUnitsOfType<FirefightersBackupUnit>();
            if (firefightersUnits != null)
            {
                foreach (FirefightersBackupUnit u in firefightersUnits)
                    u.Delete();
            }

            ParamedicsBackupUnit[] paramedicsUnits = BackupUnit.GetAllBackupUnitsOfType<ParamedicsBackupUnit>();
            if (paramedicsUnits != null)
            {
                foreach (ParamedicsBackupUnit u in paramedicsUnits)
                    u.Delete();
            }
        }

        private static void LoadControls()
        {
            Controls = LoadFileFromResourcesFolder<Controls>("Controls.xml", Controls.GetDefault);
        }

        internal static void SaveControls()
        {
            Util.Serialize<Controls>(Path.Combine(ResourcesFolder, "Controls.xml"), Controls);
        }

        private static void LoadSettings()
        {
            UserSettings = LoadFileFromResourcesFolder<Settings>("UserSettings.xml", Settings.GetDefault);
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
