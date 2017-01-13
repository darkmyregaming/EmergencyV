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
        private static void SetPlaterStateToNormal() => PlayerManager.Instance.SetPlayerToState(PlayerStateType.Normal);
        [ConsoleCommand]
        private static void SetPlaterStateToFirefighter() => PlayerManager.Instance.SetPlayerToState(PlayerStateType.Firefighter);
        [ConsoleCommand]
        private static void SetPlaterStateToEMS() => PlayerManager.Instance.SetPlayerToState(PlayerStateType.EMS);

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
                Game.Console.Print(a.GetName().Name);
            }
        }

        [ConsoleCommand]
        private static void ListCurrentAddons()
        {
            foreach (API.Addon a in AddonsManager.Instance.CurrentAddons)
            {
                Game.Console.Print(a.Name);
            }
        }
    }
}
