namespace EmergencyV
{
    // System
    using System;
    using System.Linq;

    // RPH
    using Rage;
    using Rage.Native;

    internal class PlayerFireEquipmentManager
    {
        private static PlayerFireEquipmentManager instance;
        public static PlayerFireEquipmentManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayerFireEquipmentManager();
                return instance;
            }
        }

        private readonly Vector3 FlashlightOriginOffset = new Vector3(-0.1325f, 0.2f, 0.2825f);

        public bool HasFireExtinguisher
        {
            get
            {
                if (!Plugin.LocalPlayerCharacter)
                    return false;

                return Plugin.LocalPlayerCharacter.Inventory.Weapons.Contains(WeaponHash.FireExtinguisher);
            }
            set
            {
                if (!Plugin.LocalPlayerCharacter)
                    return;

                if (value)
                    Plugin.LocalPlayerCharacter.Inventory.GiveNewWeapon(WeaponHash.FireExtinguisher, 999, true);
                else
                    Plugin.LocalPlayerCharacter.Inventory.Weapons.Remove(WeaponHash.FireExtinguisher);

            }
        }

        public bool HasFireGear
        {
            get
            {
                if (!Plugin.LocalPlayerCharacter)
                    return false;

                int drawableIndex, drawableTextureIndex;
                Plugin.LocalPlayerCharacter.GetVariation(8, out drawableIndex, out drawableTextureIndex);
                int propDrawableIndex = NativeFunction.Natives.GetPedPropIndex<int>(Plugin.LocalPlayerCharacter, 1);
                return (drawableIndex == 1 || drawableIndex == 2) && propDrawableIndex == 0;
            }
            set
            {
                if (!Plugin.LocalPlayerCharacter)
                    return;

                if (value)
                {
                    Plugin.LocalPlayerCharacter.SetVariation(8, 1, 0);
                    NativeFunction.Natives.SetPedPropIndex(Plugin.LocalPlayerCharacter, 1, 0, 0, true);
                }
                else
                {
                    Plugin.LocalPlayerCharacter.SetVariation(8, 0, 0);
                    NativeFunction.Natives.ClearPedProp(Plugin.LocalPlayerCharacter, 1);
                }
            }
        }
        public bool HasAxe { get; }

        private bool isFlashlightOn;
        public bool IsFlashlightOn
        {
            get
            {
                if (!isFlashlightOn || !HasFireGear)
                    return false;

                return isFlashlightOn;
            }
            set
            {
                if (value == isFlashlightOn || !HasFireGear)
                    return;

                isFlashlightOn = value;
                if (isFlashlightOn)
                {
                    Plugin.LocalPlayerCharacter.SetVariation(8, 2, 0);
                }
                else
                {
                    Plugin.LocalPlayerCharacter.SetVariation(8, 1, 0);
                }
            }
        }

        private PlayerFireEquipmentManager()
        {
        }

        public void Update()
        {
            if (PlayerManager.Instance.IsFirefighter)
            {
                FireFighterUpdate();
            }
        }

        private bool isNearFiretruck = false;
        private bool isGettingEquipment = false;
        private DateTime lastFiretrucksCheckTime = DateTime.UtcNow;
        private void FireFighterUpdate()
        {
            if ((DateTime.UtcNow - lastFiretrucksCheckTime).TotalSeconds > 3.25)
            {
                isNearFiretruck = IsFiretruckNearbyPlayer();
            }


            if (isGettingEquipment)
            {
                Game.DisplayHelp("[1] Fire extinguisher~n~[2] Fire gear~n~[3] Axe"); // TODO: some cool GUI for the get equipment menu

                if (Game.IsKeyDown(System.Windows.Forms.Keys.D1))
                {
                    HasFireExtinguisher = !HasFireExtinguisher;

                    isGettingEquipment = false;
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.D2))
                {
                    HasFireGear = !HasFireGear;

                    isGettingEquipment = false;
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.D3))
                {
                    // TODO: use "prop_tool_fireaxe" to give an axe to the player
                    isGettingEquipment = false;
                }
            }
            else if (isNearFiretruck)
            {
                Game.DisplayHelp("Press ~INPUT_CONTEXT~ to get/save equipment", 20);
                if (Game.IsControlJustPressed(0, GameControl.Context))
                {
                    isGettingEquipment = true;
                }
            }

            if (Game.IsKeyDown(System.Windows.Forms.Keys.L))
            {
                IsFlashlightOn = !IsFlashlightOn;
            }

            if (IsFlashlightOn)
            {
                Vector3 flashlightPos = Plugin.LocalPlayerCharacter.GetOffsetPosition(Plugin.LocalPlayerCharacter.GetPositionOffset(Plugin.LocalPlayerCharacter.GetBonePosition(PedBoneId.Spine2)) + FlashlightOriginOffset);

                Util.DrawSpotlightWithShadow(flashlightPos, Plugin.LocalPlayerCharacter.GetBoneRotation(PedBoneId.Spine2).ToVector(), System.Drawing.Color.FromArgb(15, 15, 15), 12.5f, 8.0f, 2.0f, 17.25f, 20.0f);
#if DEBUG
                Util.DrawMarker(28, flashlightPos, Vector3.Zero, Rotator.Zero, new Vector3(0.075f), System.Drawing.Color.Yellow);
#endif
            }
        }


        private bool IsFiretruckNearbyPlayer()
        {
            if (!Plugin.LocalPlayerCharacter)
                return false;

            bool isNearAnyFiretruck = false;

            Vehicle[] nearbyFiretrucks = Plugin.LocalPlayerCharacter.GetNearbyVehicles(4).Where(v => v.Model == new Model("firetruk")).ToArray();
            if (nearbyFiretrucks.Length >= 1)
            {
                for (int i = 0; i < nearbyFiretrucks.Length; i++)
                {
                    Vehicle v = nearbyFiretrucks[0];
                    if (v && Vector3.DistanceSquared(v.RearPosition, Plugin.LocalPlayerCharacter.Position) < 2.5f * 2.5f)
                    {
                        isNearAnyFiretruck = true;
                    }
                }
            }

            return isNearAnyFiretruck;
        }
    }
}
