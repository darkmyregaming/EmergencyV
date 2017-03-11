namespace EmergencyV
{
    // RPH
    using Rage;

    internal class FireExtinguisherEquipment : IFirefighterEquipment
    {
        string IFirefighterEquipment.DisplayName => "Fire Extinguisher";
        bool IFirefighterEquipment.ShouldUpdateIfEquipped => false;
        
        bool IFirefighterEquipment.IsEquipped(FirefighterEquipmentController controller)
        {
            return controller.Ped.Inventory.Weapons.Contains(WeaponHash.FireExtinguisher);
        }

        void IFirefighterEquipment.OnGetEquipment(FirefighterEquipmentController controller)
        {
            controller.Ped.Inventory.GiveNewWeapon(WeaponHash.FireExtinguisher, -1, true);
        }

        void IFirefighterEquipment.OnLeaveEquipment(FirefighterEquipmentController controller)
        {
            controller.Ped.Inventory.Weapons.Remove(WeaponHash.FireExtinguisher);
        }

        void IFirefighterEquipment.OnEquippedUpdate(FirefighterEquipmentController controller)
        {
        }
    }
}
