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
            int i = 0;
            while (true)
            {
                GameFiber.Yield();

                LocalPlayer = Game.LocalPlayer;
                LocalPlayerCharacter = LocalPlayer.Character;

                hose.Update();

                if (Game.IsKeyDown(System.Windows.Forms.Keys.D1))
                {
                    int thisId = i++;

                    GameFiber.StartNew(() =>
                    {
                        Action<string> log = (t) =>
                        {
                            Game.DisplayNotification($"#{thisId}: " + t);
                            Game.LogTrivial($"#{thisId}: " + t);
                        };

                        Firefighter f = new Firefighter(Game.LocalPlayer.Character.GetOffsetPositionFront(4f), 0.0f);
                        GameFiber.StartNew(() =>
                        {
                            while (f != null)
                            {
                                GameFiber.Yield();
                                f?.Update();
                            }
                        });

                        Vehicle vehicle = new Vehicle("asea", Game.LocalPlayer.Character.GetOffsetPositionFront(25.0f));

                        log("created firefighter and vehicle");

                        GameFiber.Sleep(1000);

                        log("starting enter vehicle task");
                        AITask enterVehicleTask = f.AI.EnterVehicle(vehicle, -1);

                        while (!enterVehicleTask.IsFinished)
                            GameFiber.Sleep(1000);

                        log("finished enter vehicle task");

                        GameFiber.Sleep(8000);

                        log("starting drive task");
                        AITask driveTask = f.AI.DriveTo(f.Ped.GetOffsetPositionFront(10.0f), 5.0f, 1.0f, VehicleDrivingFlags.Emergency);

                        while (!enterVehicleTask.IsFinished)
                            GameFiber.Sleep(1000);

                        log("finished drive task");

                        GameFiber.Sleep(8000);

                        log("starting leave vehicle task");

                        AITask leaveVehicleTask = f.AI.LeaveVehicle(LeaveVehicleFlags.None);

                        while (!enterVehicleTask.IsFinished)
                            GameFiber.Sleep(1000);

                        log("finished leave vehicle task");

                        GameFiber.Sleep(6000);

                        log("creating fires");
                        Vector3[] firesPos = new Vector3[5];
                        for (int j = 0; j < 5; j++)
                        {
                            firesPos[j] = f.Ped.GetOffsetPositionFront(8.0f).Around2D(3.5f);
                        }
                        API.ScriptedFire[] fires = Util.CreateFires(firesPos, 2, true, true);

                        log("starting extinguish fire task");

                        AITask extinguishFireTask = f.AI.ExtinguishFireInArea(f.Ped.GetOffsetPositionFront(8.0f), 4.0f, false);
                        f.Equipment.HasFireGear = true;
                        f.Equipment.IsFlashlightOn = true;

                        while (!extinguishFireTask.IsFinished)
                            GameFiber.Sleep(1000);

                        log("finished extinguish fire task");

                        GameFiber.Sleep(6500);

                        if (f.Ped)
                            f.Ped.Delete();
                        if (vehicle)
                            vehicle.Delete();
                        for (int j = 0; j < fires.Length; j++)
                        {
                            fires[j].Remove();
                        }
                        f = null;
                    });
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
