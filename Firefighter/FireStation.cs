namespace EmergencyV
{
    // System
    using System;
    using System.Linq;
    using System.Drawing;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class FireStation : Building<FireStationData>
    {
        public Vehicle Engine { get; private set; }
        public RotatedVector3 EngineSpawn { get; }

        public Vehicle Battalion { get; private set; }
        public RotatedVector3 BattalionSpawn { get; }

        public Vehicle Rescue { get; private set; }
        public RotatedVector3 RescueSpawn { get; }

        protected override Color BlipColor { get { return Color.FromArgb(180, 0, 0); } }
        protected override BlipSprite BlipSprite { get { return BlipSprite.CriminalCarsteal; } }
        protected override string BlipName { get { return "Fire Station"; } }

        public bool IsAlarmPlaying { get { return alarmCloseSoundId != -1 || alarmFarSoundId != -1; } }

        public ParkingSpot[] ParkingSpots { get; }
        public FirefightersUnit[] Units { get; }

        public FireStation(FireStationData data) : base(data)
        {
            Game.LogTrivialDebug("Loaded " + data.Name);
            EngineSpawn = new RotatedVector3(data.EngineSpawn.ToVector3(), new Rotator(0f, 0f, data.EngineSpawn.W));
            BattalionSpawn = new RotatedVector3(data.BattalionSpawn.ToVector3(), new Rotator(0f, 0f, data.BattalionSpawn.W));
            RescueSpawn = new RotatedVector3(data.RescueSpawn.ToVector3(), new Rotator(0f, 0f, data.RescueSpawn.W));

            if (data.ParkingSpots == null)
            {
                ParkingSpots = new ParkingSpot[0];
                Units = new FirefightersUnit[0];
            }
            else
            {
                ParkingSpots = new ParkingSpot[data.ParkingSpots.Length];
                Units = new FirefightersUnit[data.ParkingSpots.Length];
                for (int i = 0; i < data.ParkingSpots.Length; i++)
                {
                    ParkingSpots[i] = new ParkingSpot(data.ParkingSpots[i].ToVector3(), data.ParkingSpots[i].W);
                    Units[i] = new FirefightersUnit(this);
                    ParkingSpots[i].CurrentUnit = Units[i];
                    ParkingSpots[i].IsOccupied = true;
                }
            }
        }

        protected override void CreateInternal()
        {
            Engine = new Vehicle(Plugin.UserSettings.VEHICLES.ENGINE_MODEL, EngineSpawn.Position, EngineSpawn.Heading);
            if (Plugin.UserSettings.VEHICLES.ENGINE_PRIMARY_COLOR != null) Engine.PrimaryColor = Plugin.UserSettings.VEHICLES.ENGINE_PRIMARY_COLOR.ToColor();
            if (Plugin.UserSettings.VEHICLES.ENGINE_SECONDARY_COLOR != null) Engine.SecondaryColor = Plugin.UserSettings.VEHICLES.ENGINE_SECONDARY_COLOR.ToColor();


            Rescue = new Vehicle(Plugin.UserSettings.VEHICLES.RESCUE_MODEL, RescueSpawn.Position, RescueSpawn.Heading);
            if (Plugin.UserSettings.VEHICLES.RESCUE_PRIMARY_COLOR != null) Rescue.PrimaryColor = Plugin.UserSettings.VEHICLES.RESCUE_PRIMARY_COLOR.ToColor();
            if (Plugin.UserSettings.VEHICLES.RESCUE_SECONDARY_COLOR != null) Rescue.SecondaryColor = Plugin.UserSettings.VEHICLES.RESCUE_SECONDARY_COLOR.ToColor();


            Battalion = new Vehicle(Plugin.UserSettings.VEHICLES.BATTALION_MODEL, BattalionSpawn.Position, BattalionSpawn.Heading);
            if (Plugin.UserSettings.VEHICLES.BATTALION_PRIMARY_COLOR != null) Battalion.PrimaryColor = Plugin.UserSettings.VEHICLES.BATTALION_PRIMARY_COLOR.ToColor();
            if (Plugin.UserSettings.VEHICLES.BATTALION_SECONDARY_COLOR != null) Battalion.SecondaryColor = Plugin.UserSettings.VEHICLES.BATTALION_SECONDARY_COLOR.ToColor();
        }

        protected override void DeleteInternal()
        {
            if (Engine)
                Engine.Dismiss();
            if (Battalion)
                Battalion.Dismiss();
            if (Rescue)
                Rescue.Dismiss();
            StopAlarm();
        }

        protected override void CleanUpInternal()
        {
            foreach (FirefightersUnit u in Units)
                u.DismissEntities();
        }

        private int alarmCloseSoundId = -1, alarmFarSoundId = -1;
        public void StartAlarm()
        {
            while (!NativeFunction.Natives.RequestScriptAudioBank<bool>("alarm_klaxon_05", true))// alarm_klaxon_05
                GameFiber.Sleep(10);

            alarmCloseSoundId = NativeFunction.Natives.GetSoundId<int>();
            alarmFarSoundId = NativeFunction.Natives.GetSoundId<int>();

            NativeFunction.Natives.PlaySoundFromCoord(alarmCloseSoundId, "ALARMS_KLAXON_05_CLOSE", Entrance.X, Entrance.Y, Entrance.Z, 0, false, 0, false);//ALARMS_KLAXON_05_FAR
            NativeFunction.Natives.PlaySoundFromCoord(alarmFarSoundId, "ALARMS_KLAXON_05_FAR", Entrance.X, Entrance.Y, Entrance.Z, 0, false, 0, false);
        }

        public void StartAlarm(int milliseconds)
        {
            StartAlarm();

            GameFiber.StartNew(() =>
            {
                GameFiber.Sleep(milliseconds);

                StopAlarm();
            });
        }

        public void StopAlarm()
        {
            if (alarmCloseSoundId != -1)
            {
                NativeFunction.Natives.StopSound(alarmCloseSoundId);
                NativeFunction.Natives.ReleaseSoundId(alarmCloseSoundId);
            }

            if (alarmFarSoundId != -1)
            {
                NativeFunction.Natives.StopSound(alarmFarSoundId);
                NativeFunction.Natives.ReleaseSoundId(alarmFarSoundId);
            }

            alarmCloseSoundId = -1;
            alarmFarSoundId = -1;
        }

        public Vehicle GetVehicleForRole(FirefighterRole role)
        {
            switch (role)
            {
                default:
                case FirefighterRole.None: return null;
                case FirefighterRole.Engine: return Engine;
                case FirefighterRole.Battalion: return Battalion;
                case FirefighterRole.Rescue: return Rescue;
            }
        }

        public RotatedVector3 GetVehicleLocationForRole(FirefighterRole role)
        {
            switch (role)
            {
                default:
                case FirefighterRole.None: return new RotatedVector3();
                case FirefighterRole.Engine: return EngineSpawn;
                case FirefighterRole.Battalion: return BattalionSpawn;
                case FirefighterRole.Rescue: return RescueSpawn;
            }
        }

        public ParkingSpot GetFreeParkingSpot()
        {
            return ParkingSpots.FirstOrDefault(s => !s.IsOccupied);
        }


        public class ParkingSpot
        {
            public RotatedVector3 Location { get; }

            public bool IsOccupied { get; internal set; }
            public FirefightersUnit CurrentUnit { get; internal set; }

            public ParkingSpot(Vector3 position, float heading)
            {
                Location = new RotatedVector3(position, new Rotator(0f, 0f, heading));
            }
        }
    }
}
