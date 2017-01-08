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

            NotificationsManager.Instance.StartFiber();
            RespawnController.Instance.StartFiber();

            AddonsManager.Instance.LoadAddons();

            HoseTest hose = new HoseTest();
            while (true)
            {
                GameFiber.Yield();

                LocalPlayer = Game.LocalPlayer;
                LocalPlayerCharacter = LocalPlayer.Character;

                hose.Update();

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

        private static int testId = 0;
        private static void TestFirefighterAI()
        {
            int thisId = testId++;

            GameFiber.StartNew(() =>
            {
                Action<string> log = (t) =>
                {
                    Game.DisplayNotification($"#{thisId}: " + t);
                    Game.LogTrivial($"#{thisId}: " + t);
                };

                Firefighter f = new Firefighter(Game.LocalPlayer.Character.GetOffsetPositionFront(4f), 0.0f);

                Vehicle vehicle = new Vehicle("firetruk", Game.LocalPlayer.Character.GetOffsetPositionFront(50.0f));

                log("created firefighter and vehicle");

                GameFiber.Sleep(1000);

                log("starting enter vehicle task");
                AITask enterVehicleTask = f.AI.EnterVehicle(vehicle, -1);

                while (!enterVehicleTask.IsFinished)
                    GameFiber.Sleep(1000);

                log("finished enter vehicle task");

                GameFiber.Sleep(8000);

                log("starting drive task");
                NativeFunction.Natives.SetDriverAbility(f.Ped, 1.0f);
                NativeFunction.Natives.SetDriverAggressiveness(f.Ped, 0.0f);
                Vector3 pos = World.GetNextPositionOnStreet(f.Ped.Position.Around2D(200.0f, 1000.0f));
                AITask driveTask = f.AI.DriveTo(pos, 32.5f, 10.0f, VehicleDrivingFlags.Emergency);
                Blip blip = new Blip(pos);
                vehicle.IsSirenOn = true;

                while (!driveTask.IsFinished)
                    GameFiber.Sleep(1000);

                if (blip) blip.Delete();

                log("finished drive task");

                GameFiber.Sleep(8000);
                
                log("starting leave vehicle task");

                AITask leaveVehicleTask = f.AI.LeaveVehicle(LeaveVehicleFlags.None);

                while (!leaveVehicleTask.IsFinished)
                    GameFiber.Sleep(1000);

                f.Ped.PlayAmbientSpeech("EMERG_ARRIVE_ON_SCENE", false);

                log("finished leave vehicle task");

                GameFiber.Sleep(6000);

                log("creating fires");
                Vector3[] firesPos = new Vector3[5];
                for (int j = 0; j < 5; j++)
                {
                    firesPos[j] = f.Ped.GetOffsetPositionFront(6.0f).Around2D(2f);
                }
                API.ScriptedFire[] fires = Util.CreateFires(firesPos, 2, false, true);

                log("starting extinguish fire task");

                f.Ped.PlayAmbientSpeech("PUTTING_OUT_FIRE", false);

                AITask extinguishFireTask = f.AI.ExtinguishFireInArea(f.Ped.GetOffsetPositionFront(6.0f), 15.0f, true);
                f.Equipment.HasFireGear = true;
                f.Equipment.IsFlashlightOn = true;

                while (!extinguishFireTask.IsFinished)
                    GameFiber.Sleep(1000);

                f.Ped.PlayAmbientSpeech("FIRE_IS_OUT", false);
                log("finished extinguish fire task");

                GameFiber.Sleep(6500);

                if (f.Ped) f.Ped.Delete();
                if (vehicle) vehicle.Delete();
                if (blip) blip.Delete();
                for (int j = 0; j < fires.Length; j++)
                {
                    fires[j].Fire.Delete();
                }
                f = null;
            });
        }
    }
}
