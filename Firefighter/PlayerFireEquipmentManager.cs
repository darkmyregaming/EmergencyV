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
                    Plugin.LocalPlayerCharacter.Inventory.GiveNewWeapon(WeaponHash.FireExtinguisher, -1, true);
                else
                    Plugin.LocalPlayerCharacter.Inventory.Weapons.Remove(WeaponHash.FireExtinguisher);

            }
        }

        private bool hasFireGear;
        public bool HasFireGear
        {
            get
            {
                return hasFireGear;
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
                hasFireGear = value;
            }
        }

        //public bool HasAxe { get; }

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
                        foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ON_COMPONENTS)
                        {
                            Plugin.LocalPlayerCharacter.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
                        }
                        foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ON_PROPS)
                        {
                            NativeFunction.Natives.SetPedPropIndex(Plugin.LocalPlayerCharacter, var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex, true);
                        }

                    }
                    else
                    {
                        foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_OFF_COMPONENTS)
                        {
                            Plugin.LocalPlayerCharacter.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
                        }
                        foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ON_PROPS)
                        {
                            NativeFunction.Natives.ClearPedProp(Plugin.LocalPlayerCharacter, var.ComponentIndex);
                        }
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
        private DateTime lastFiretrucksCheckTime = DateTime.UtcNow;
        private void FireFighterUpdate()
        {
            if ((DateTime.UtcNow - lastFiretrucksCheckTime).TotalSeconds > 3.25)
            {
                bool nearFiretruckNow = IsFiretruckNearbyPlayer();

                if (isNearFiretruck != nearFiretruckNow)
                {
                    if (nearFiretruckNow)
                    {
                        CreateVehicleEquipmentMenu();
                    }
                    else
                    {
                        RemoveVehicleEquipmentMenu();
                    }
                }

                isNearFiretruck = nearFiretruckNow;
            }

            if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
            {
                if (IsFlashlightOn)
                {
                    Vector3 flashlightPos = Plugin.LocalPlayerCharacter.GetOffsetPosition(Plugin.LocalPlayerCharacter.GetPositionOffset(Plugin.LocalPlayerCharacter.GetBonePosition(Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE)) + Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET.ToVector3());

                    Util.DrawSpotlightWithShadow(flashlightPos, Plugin.LocalPlayerCharacter.GetBoneRotation(PedBoneId.Spine2).ToVector(), Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_COLOR.ToColor(), 13.25f, 9.25f, 2.0f, 20f, 20.0f);
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

        private void CreateVehicleEquipmentMenu()
        {
            PluginMenu.Instance.AddMenu("VEHICLE_EQUIPMENT_SUBMENU");

            PluginMenu.Instance.AddItem("OPEN_VEHICLE_EQUIPMENT_SUBMENU_ITEM", "MAIN_MENU", "Equipment", null, null, "VEHICLE_EQUIPMENT_SUBMENU");

            PluginMenu.Instance.AddItem("VEHICLE_EQUIPMENT_FIRE_GEAR_ITEM", "VEHICLE_EQUIPMENT_SUBMENU", HasFireGear ? "Leave Fire Gear" : "Get Fire Gear", () => { HasFireGear = !HasFireGear; });
            PluginMenu.Instance.AddItem("VEHICLE_EQUIPMENT_FIRE_EXTINGUISHER_ITEM", "VEHICLE_EQUIPMENT_SUBMENU", HasFireGear ? "Leave Fire Extinguisher" : "Get Fire Extinguisher", () => { HasFireExtinguisher = !HasFireExtinguisher; });
        }

        private void RemoveVehicleEquipmentMenu()
        {
            PluginMenu.Instance.RemoveMenu("VEHICLE_EQUIPMENT_SUBMENU");

            PluginMenu.Instance.RemoveItem("OPEN_VEHICLE_EQUIPMENT_SUBMENU_ITEM");
        }
    }
}
