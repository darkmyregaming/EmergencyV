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
                    foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_ENABLED_COMPONENTS)
                    {
                        Plugin.LocalPlayerCharacter.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
                    }
                    foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_ENABLED_PROPS)
                    {
                        NativeFunction.Natives.SetPedPropIndex(Plugin.LocalPlayerCharacter, var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex, true);
                    }

                }
                else
                {
                    foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_DISABLED_COMPONENTS)
                    {
                        Plugin.LocalPlayerCharacter.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
                    }
                    foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_ENABLED_PROPS)
                    {
                        NativeFunction.Natives.ClearPedProp(Plugin.LocalPlayerCharacter, var.ComponentIndex);
                    }
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
                if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
                {
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

            if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
            {
                if (Game.IsKeyDown(System.Windows.Forms.Keys.L))
                {
                    IsFlashlightOn = !IsFlashlightOn;
                }

                if (IsFlashlightOn)
                {
                    Vector3 flashlightPos = Plugin.LocalPlayerCharacter.GetOffsetPosition(Plugin.LocalPlayerCharacter.GetPositionOffset(Plugin.LocalPlayerCharacter.GetBonePosition(Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE)) + Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET.ToVector3());

                    Util.DrawSpotlightWithShadow(flashlightPos, Plugin.LocalPlayerCharacter.GetBoneRotation(PedBoneId.Spine2).ToVector(), Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_COLOR.ToColor(), 13.25f, 9.25f, 2.0f, 20f, 20.0f);
#if DEBUG
                    Util.DrawMarker(28, flashlightPos, Vector3.Zero, Rotator.Zero, new Vector3(0.075f), System.Drawing.Color.Yellow);
#endif
                }
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
