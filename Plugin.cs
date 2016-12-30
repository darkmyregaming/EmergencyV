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

        private static Controls controls;
        public static Controls Controls { get { return controls; } }

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
            List<Firefighter> firefighters = new List<Firefighter>();
            while (true)
            {
                GameFiber.Yield();

                LocalPlayer = Game.LocalPlayer;
                LocalPlayerCharacter = LocalPlayer.Character;

                hose.Update();

                // firefighters AI testing code
                foreach (Firefighter f in firefighters)
                {
                    f?.Update();
                }

                if (Game.IsKeyDown(System.Windows.Forms.Keys.D1))
                {
                    firefighters.Add(new Firefighter(Plugin.LocalPlayerCharacter.GetOffsetPositionFront(5f), 0.0f));
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.D2))
                {
                    Vector3 v = Plugin.LocalPlayerCharacter.GetOffsetPositionFront(8f);

                    Vector3[] ps = new Vector3[25];
                    for (int i = 0; i < 25; i++)
                    {
                        ps[i] = v.Around2D(1.0f, 6.0f);
                    }

                    API.ScriptedFire[] fires = Util.CreateFires(ps, 25, false, true);
                    foreach (API.ScriptedFire fi in fires)
                    {
                        fi.Fire.DesiredBurnDuration = 45.0f;
                    }

                    GameFiber.StartNew(() =>
                    {
                        GameFiber.Sleep(180000);
                        foreach (API.ScriptedFire fi in fires)
                        {
                            fi.Remove();
                        }
                    });
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.D3))
                {
                    foreach (Firefighter f in firefighters)
                    {
                        if (f != null && f.Ped.Exists() && !f.AI.IsPerformingTaskOfType<AIFirefighterTaskExtinguishFireInArea>())
                        {
                            f.Equipment.HasFireGear = true;
                            f.Equipment.IsFlashlightOn = true;
                            f.Equipment.HasFireExtinguisher = true;
                            f.AI.ExtinguishFireInArea(f.Ped.Position, 75.0f);
                        }
                    }
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.D4))
                {
                    foreach (Firefighter f in firefighters)
                    {
                        if (f != null && f.Ped.Exists())
                        {
                            f.AI.RunTo(LocalPlayerCharacter.Position, f.Ped.Position.GetHeadingTowards(LocalPlayerCharacter), 6.0f);
                        }
                    }
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.D5))
                {
                    foreach (Firefighter f in firefighters)
                    {
                        GameFiber.StartNew(() =>
                        {
                            Ped p = new Ped(f.Ped.GetOffsetPositionFront(5f));
                            p.BlockPermanentEvents = true;
                            p.Kill();
                            GameFiber.Sleep(1000);
                            f.AI.PerformCPR(p);
                        });
                    }
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.D6))
                {
                    foreach (Firefighter f in firefighters)
                    {
                        if (f != null && f.Ped.Exists())
                        {
                            f.Ped.Delete();
                        }
                    }
                    firefighters.Clear();
                }

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
            //PlayerEquipmentManager.Instance.CleanUp(isTerminating);
            FireCalloutsManager.Instance.CleanUp(isTerminating);

            HospitalsManager.Instance.CleanUp(isTerminating);
            EMSCalloutsManager.Instance.CleanUp(isTerminating);
        }

        private static void LoadControls()
        {
            controls = LoadFileFromResourcesFolder<Controls>("Controls.xml", Controls.GetDefault);
        }

        internal static void SaveControls()
        {
            Util.Serialize<Controls>(Path.Combine(ResourcesFolder, "Controls.xml"), controls);
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
