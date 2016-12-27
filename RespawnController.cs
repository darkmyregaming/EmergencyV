namespace EmergencyV
{
    // System
    using System.Linq;

    // RPH
    using Rage;

    internal class RespawnController
    {
        private static RespawnController instance;
        public static RespawnController Instance
        {
            get
            {
                if (instance == null)
                    instance = new RespawnController();
                return instance;
            }
        }

        private GameFiber Fiber { get; }

        private RespawnController()
        {
            Fiber = new GameFiber(ControllerLoop, "Respawn Controller Fiber");
        }

        public void StartFiber()
        {
            Fiber.Start();
        }

        private void ControllerLoop()
        {
            Vector3 deadPosition;
            while (true)
            {
                GameFiber.Yield();

                // Only runs if the player is on duty
                if (!PlayerManager.Instance.IsFirefighter && !PlayerManager.Instance.IsEMS)
                    continue;

                // Get the player's current character.
                Ped playerPed = Game.LocalPlayer.Character;

                // Sleep while the player ped exists and is alive.
                while (playerPed && playerPed.IsAlive)
                {
                    GameFiber.Sleep(100);
                }

                // If the player ped was deleted, wait for a new one, and start over.
                if (!playerPed)
                {
                    // Wait for the player to receive a new character.
                    while (!Game.LocalPlayer.Character)
                    {
                        GameFiber.Yield();
                    }

                    // Restart loop.
                    continue;
                }

                // Disable the automatic respawn system.
                Game.DisableAutomaticRespawn = true;

                // Disable fade out on death.
                Game.FadeScreenOutOnDeath = false;
                
                deadPosition = playerPed.Position;

                // Sleep four seconds after the player ped has died (let the game's dead message show up).
                GameFiber.Sleep(4000);

                // Fade the screen out.
                Game.FadeScreenOut(500, true);

                GameFiber.Sleep(10);

                // Resurrect the player.
                playerPed.Resurrect();

                // Get the closest fire station/hospital entrance position, it will be where the player respawns
                Vector3? closestRespawnPoint = PlayerManager.Instance.IsFirefighter ? FireStationsManager.Instance.Buildings.OrderBy(s => Vector3.DistanceSquared(s.Entrance, deadPosition)).FirstOrDefault()?.Entrance :
                                               PlayerManager.Instance.IsEMS ? HospitalsManager.Instance.Buildings.OrderBy(s => Vector3.DistanceSquared(s.Entrance, deadPosition)).FirstOrDefault()?.Entrance :
                                               null;

                if (closestRespawnPoint.HasValue)
                {
                    playerPed.SetPositionWithSnap(closestRespawnPoint.Value);
                }
                

                GameFiber.Sleep(10);

                // Reset the player state, HUD, menus, etc.
                Game.HandleRespawn();

                // Fade the screen in.
                Game.FadeScreenIn(250, true);

                // Continue loop (Do things all over again).
            }
        }
    }
}
