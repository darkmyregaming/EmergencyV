namespace EmergencyV
{
    // RPH
    using Rage;
    using Rage.Native;

    internal class PluginMenu
    {
        private static PluginMenu instance;
        public static PluginMenu Instance
        {
            get
            {
                if (instance == null)
                    instance = new PluginMenu();
                return instance;
            }
        }

        public Menu MainMenu { get; }
        public Menu ActionsSubmenu { get; }
        public Menu RequestBackupSubmenu { get; }

        private PluginMenu()
        {
            MainMenu = new Menu();
            ActionsSubmenu = new Menu() { ParentMenu = MainMenu };
            RequestBackupSubmenu = new Menu() { ParentMenu = MainMenu };

            MainMenu.Items.Add(new MenuItem("Actions", null) { BindedSubmenu = ActionsSubmenu });
            MainMenu.Items.Add(new MenuItem("Request Backup", null) { BindedSubmenu = RequestBackupSubmenu });


            if (Plugin.UserSettings.PEDS.FIREFIGHTER_FLASHLIGHT_ENABLED)
                ActionsSubmenu.Items.Add(new MenuItem("Toggle Flashlight", () => { PlayerFireEquipmentManager.Instance.IsFlashlightOn = !PlayerFireEquipmentManager.Instance.IsFlashlightOn; }, Plugin.Controls["TOGGLE_FLASHLIGHT"]));

            RequestBackupSubmenu.Items.Add(new MenuItem("[WIP]", () => { Game.DisplaySubtitle("[WIP]"); }));
        }

        public void Update()
        {
            if (PlayerManager.Instance.PlayerState != PlayerStateType.Normal && Plugin.Controls["OPEN_MENU"].IsJustPressed())
                MainMenu.IsVisible = !MainMenu.IsVisible;
        }
    }
}
