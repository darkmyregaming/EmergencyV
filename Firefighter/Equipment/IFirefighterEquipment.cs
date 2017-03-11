namespace EmergencyV
{
    internal interface IFirefighterEquipment
    {
        string DisplayName { get; }
        bool ShouldUpdateIfEquipped { get; }
        bool IsEquipped(FirefighterEquipmentController controller);
        void OnGetEquipment(FirefighterEquipmentController controller);
        void OnLeaveEquipment(FirefighterEquipmentController controller);
        void OnEquippedUpdate(FirefighterEquipmentController controller);
    }
}
