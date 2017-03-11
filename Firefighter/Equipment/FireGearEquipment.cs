namespace EmergencyV
{
    // RPH
    using Rage;
    using Rage.Native;

    internal class FireGearEquipment : IFirefighterEquipment
    {
        public const string IsFireGearEquippedMemoryKey = "IsFireGearEquipped";

        string IFirefighterEquipment.DisplayName => "Fire Gear";
        bool IFirefighterEquipment.ShouldUpdateIfEquipped => false;

        bool IFirefighterEquipment.IsEquipped(FirefighterEquipmentController controller)
        {
            return (bool)controller.Memory.GetOrDefault(IsFireGearEquippedMemoryKey, false);
        }

        void IFirefighterEquipment.OnGetEquipment(FirefighterEquipmentController controller)
        {
            Ped ped = controller.Ped;

            foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_ENABLED_COMPONENTS)
            {
                ped.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
            }
            foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_ENABLED_PROPS)
            {
                NativeFunction.Natives.SetPedPropIndex(ped, var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex, true);
            }

            ped.IsFireProof = true;

            controller.Memory.SetOrAdd(IsFireGearEquippedMemoryKey, true);
        }

        void IFirefighterEquipment.OnLeaveEquipment(FirefighterEquipmentController controller)
        {
            Ped ped = controller.Ped;

            foreach (Settings.PedComponentVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_DISABLED_COMPONENTS)
            {
                ped.SetVariation(var.ComponentIndex, var.DrawableIndex, var.DrawableTextureIndex);
            }
            foreach (Settings.PedPropVariation var in Plugin.UserSettings.PEDS.FIREFIGHTER_FIRE_GEAR_ENABLED_PROPS)
            {
                NativeFunction.Natives.ClearPedProp(ped, var.ComponentIndex);
            }

            ped.IsFireProof = false;

            controller.Memory.SetOrAdd(IsFireGearEquippedMemoryKey, false);
        }

        void IFirefighterEquipment.OnEquippedUpdate(FirefighterEquipmentController controller)
        {
        }
    }
}
