namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class FireStation
    {
        public delegate void PlayerEnterFireStationEventHandler(FireStation station);

        public readonly FireStationData Data;

        public event PlayerEnterFireStationEventHandler PlayerEntered;

        public Blip Blip { get; }

        public Vehicle Engine { get; private set; }
        public Vehicle Battalion { get; private set; }
        public Vehicle Rescue { get; private set; }

        public float ActivationRange { get; set; } = 80.0f;
        public bool IsCreated { get; private set; }

        public FireStation(FireStationData data)
        {
            Data = data;

            Blip = new Blip(Data.EntrancePosition);
            Blip.Sprite = BlipSprite.CriminalCarsteal;
            Blip.Color = Color.FromArgb(180, 0, 0);
            Blip.Name = "Fire Station";
            NativeFunction.Natives.SetBlipAsShortRange(Blip, true);
        }

        public void Update(bool shouldPlayerEnterStationIfNear = false)
        {
            if (!IsCreated)
                return;

            if (PlayerManager.Instance.PlayerState == PlayerStateType.Normal)
            {
                if (Vector3.DistanceSquared(Data.EntrancePosition, Plugin.LocalPlayerCharacter.Position) < 2.0f * 2.0f)
                {
                    Game.DisplayHelp("Press ~INPUT_CONTEXT~ to enter", 20);
                    if (shouldPlayerEnterStationIfNear)
                    {
                        OnPlayerEntered();
                    }
                }

                Util.DrawMarker(0, Data.EntrancePosition, Vector3.Zero, new Rotator(0.0f, 0.0f, 0.0f), new Vector3(1f), Color.FromArgb(150, Color.Yellow), true);
            }
        }

        public void Create()
        {
            Delete();
            Game.LogTrivial("Creating station with name " + Data.Name);

            Engine = new Vehicle("firetruk", Data.EngineLocation.Position);
            Engine.Rotation = Data.EngineLocation.Rotation;
            Battalion = new Vehicle("fbi2", Data.BattalionLocation.Position);
            Battalion.Rotation = Data.BattalionLocation.Rotation;
            Battalion.PrimaryColor = Color.FromArgb(200, 0, 0);
            Rescue = new Vehicle("firetruk", Data.RescueLocation.Position);
            Rescue.Rotation = Data.RescueLocation.Rotation;

            IsCreated = true;
        }

        public void Delete()
        {
            Game.LogTrivial("Deleting station with name " + Data.Name);

            if (Engine)
                Engine.Dismiss();
            if (Battalion)
                Battalion.Dismiss();
            if (Rescue)
                Rescue.Dismiss();

            IsCreated = false;
        }

        public void CleanUp()
        {
            Delete();
            if (Blip)
                Blip.Delete();
        }

        public bool IsInActivationRangeFrom(Vector3 position)
        {
            return Vector3.DistanceSquared(Data.EntrancePosition, position) < ActivationRange * ActivationRange;
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
                case FirefighterRole.Engine: return Data.EngineLocation;
                case FirefighterRole.Battalion: return Data.BattalionLocation;
                case FirefighterRole.Rescue: return Data.RescueLocation;
            }
        }

        protected void OnPlayerEntered()
        {
            PlayerEntered?.Invoke(this);
        }
    }
}
