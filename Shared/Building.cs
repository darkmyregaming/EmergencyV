namespace EmergencyV
{
    // System
    using System.Drawing;

    // RPH
    using Rage;
    using Rage.Native;

    internal abstract class Building<TData> where TData : BuildingData
    {
        public delegate void PlayerEnteredBuildingEventHandler<TBuilding>(TBuilding building) where TBuilding : Building<TData>;

        public string Name { get; }

        public Vector3 Entrance { get; }
        public float ActivationRange { get; }


        public Blip Blip { get; }
        protected abstract BlipSprite BlipSprite { get; }
        protected abstract string BlipName { get; }
        protected abstract Color BlipColor { get; }

        public bool IsCreated { get; private set; }

        public event PlayerEnteredBuildingEventHandler<Building<TData>> PlayerEntered;

        public Building(TData data)
        {
            Name = data.Name;
            Entrance = data.Entrance.ToVector3();
            ActivationRange = data.ActivationRange;

            Blip = new Blip(Entrance);
            Blip.Sprite = BlipSprite;
            Blip.Color = BlipColor;
            Blip.Name = BlipName;
            NativeFunction.Natives.SetBlipAsShortRange(Blip, true);
        }

        public void Update()
        {
            UpdateInternal();

            if (!IsCreated || PlayerManager.Instance.PlayerState != PlayerStateType.Normal)
                return;

            if (Vector3.DistanceSquared(Entrance, Game.LocalPlayer.Character.Position) < 2.0f * 2.0f)
            {
                if (Game.IsControlJustPressed(0, GameControl.Context))
                {
                    OnPlayerEntered();
                }
                Game.DisplayHelp("Press ~INPUT_CONTEXT~ to enter", 10);
            }

            Util.DrawMarker(0, Entrance, Vector3.Zero, Rotator.Zero, new Vector3(1f), Color.FromArgb(150, Color.Yellow), true);
        }

        public void Create()
        {
            if (IsCreated)
                Delete();
            Game.LogTrivial($"Creating {this}" );

            CreateInternal();

            IsCreated = true;
        }

        public void Delete()
        {
            Game.LogTrivial($"Deleting {this}");

            DeleteInternal();

            IsCreated = false;
        }

        public void CleanUp()
        {
            Delete();
            if (Blip)
                Blip.Delete();
            CleanUpInternal();
        }

        protected virtual void UpdateInternal() { } // called continuously
        protected virtual void CreateInternal() { } // called when player enters the ActivationRange
        protected virtual void DeleteInternal() { } // called when player leave the ActivationRange
        protected virtual void CleanUpInternal() { } // called when the plugins unloads

        public bool IsInActivationRangeFrom(Vector3 position)
        {
            return Vector3.DistanceSquared(Entrance, position) < ActivationRange * ActivationRange;
        }

        protected virtual void OnPlayerEntered()
        {
            PlayerEntered?.Invoke(this);
        }

        public override string ToString()
        {
            return $"{Name} ({Entrance})";
        }
    }
}
