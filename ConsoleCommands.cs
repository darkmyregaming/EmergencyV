namespace EmergencyV
{
    // System
    using System.Reflection;

    // RPH
    using Rage;
    using Rage.Attributes;

    internal static class ConsoleCommands
    {
        [ConsoleCommand]
        private static void SetPlayerStateToNormal() => PlayerManager.Instance.SetPlayerToState(PlayerStateType.Normal);
        [ConsoleCommand]
        private static void SetPlayerStateToFirefighter() => PlayerManager.Instance.SetPlayerToState(PlayerStateType.Firefighter);
        [ConsoleCommand]
        private static void SetPlayerStateToEMS() => PlayerManager.Instance.SetPlayerToState(PlayerStateType.EMS);

        [ConsoleCommand]
        private static void SetFirefighterRoleToEngine() => PlayerManager.Instance.FirefighterRole = FirefighterRole.Engine;
        [ConsoleCommand]
        private static void SetFirefighterRoleToRescue() => PlayerManager.Instance.FirefighterRole = FirefighterRole.Rescue;
        [ConsoleCommand]
        private static void SetFirefighterRoleToBattalion() => PlayerManager.Instance.FirefighterRole = FirefighterRole.Battalion;

        [ConsoleCommand]
        private static void ListLoadedAddonsAssemblies()
        {
            foreach (Assembly a in AddonsManager.Instance.LoadedAssemblies)
            {
                Game.Console.Print("    " + a.FullName);
            }
        }

        [ConsoleCommand]
        private static void ListCurrentAddons()
        {
            foreach (API.Addon a in AddonsManager.Instance.CurrentAddons)
            {
                Game.Console.Print("    " + a.GetType().FullName);
            }
        }

#if DEBUG
        [ConsoleCommand]
        private static void Debug_CurrentUpdateFibersCount() => Game.Console.Print(UpdateInstancesFibersManager.Instance.CurrentFibersCount.ToString());

        [ConsoleCommand]
        private static void Debug_CreatePartner() => FirefighterPartner.CreatePartner(Game.LocalPlayer.Character.Position, 0.0f);

        [ConsoleCommand]
        private static void Debug_DeleteAllPartners()
        {
            FirefighterPartner[] partners = FirefighterPartner.GetAllPartners();
            foreach (FirefighterPartner p in partners)
            {
                if(p.Firefighter.Ped)
                    p.Firefighter.Ped.Delete();
            }
        }
#endif
    }
}
