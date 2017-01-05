namespace EmergencyV
{
    // RPH
    using Rage;
    using Rage.Native;

    internal class FirefighterEquipmentController
    {
        public Ped Ped { get; protected set; }

        public virtual bool HasFireExtinguisher
        {
            get
            {
                if (!Ped)
                    return false;

                return Ped.Inventory.Weapons.Contains(WeaponHash.FireExtinguisher);
            }
            set
            {
                if (!Ped)
                    return;

                if (value)
                    Ped.Inventory.GiveNewWeapon(WeaponHash.FireExtinguisher, -1, true);
                else
                    Ped.Inventory.Weapons.Remove(WeaponHash.FireExtinguisher);

            }
        }

        private bool hasFireGear;
        public virtual bool HasFireGear
        {
            get
            {
                return hasFireGear;
            }
            set
            {
                if (!Ped)
                    return;

                if (value)
                {
                    foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_ENABLED_COMPONENTS)
                    {
                        Ped.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
                    }
                    foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_ENABLED_PROPS)
                    {
                        NativeFunction.Natives.SetPedPropIndex(Ped, var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex, true);
                    }

                }
                else
                {
                    foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_DISABLED_COMPONENTS)
                    {
                        Ped.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
                    }
                    foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_ENABLED_PROPS)
                    {
                        NativeFunction.Natives.ClearPedProp(Ped, var.ComponentIndex);
                    }
                }
                Ped.IsFireProof = value;
                hasFireGear = value;
            }
        }

        private bool isFlashlightOn;
        public virtual bool IsFlashlightOn
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
                            Ped.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
                        }
                        foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ON_PROPS)
                        {
                            NativeFunction.Natives.SetPedPropIndex(Ped, var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex, true);
                        }

                    }
                    else
                    {
                        foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_OFF_COMPONENTS)
                        {
                            Ped.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
                        }
                        foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ON_PROPS)
                        {
                            NativeFunction.Natives.ClearPedProp(Ped, var.ComponentIndex);
                        }
                    }
                }
            }
        }

        public FirefighterEquipmentController(Ped ped)
        {
            Ped = ped;
        }

        public virtual void Update()
        {
            if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
            {
                if (IsFlashlightOn)
                {
                    Vector3 flashlightPos = Ped.GetOffsetPosition(Ped.GetPositionOffset(Ped.GetBonePosition(Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_BONE)) + Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ORIGIN_OFFSET.ToVector3());

                    Util.DrawSpotlightWithShadow(flashlightPos, Ped.GetBoneRotation(PedBoneId.Spine2).ToVector(), Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_COLOR.ToColor(), 13.25f, 9.25f, 2.0f, 20f, 20.0f);
                }
            }
        }
    }
}
