namespace EmergencyV
{
    // System
    using System;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal static class Plugin
    {
        public static readonly Random Random = new Random();

        public static Player LocalPlayer;
        public static Ped LocalPlayerCharacter;

        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(500);

            MathHelper.RandomizationSeed = Environment.TickCount;

            //HoseTest hose = new HoseTest();
            while (true)
            {
                GameFiber.Yield();

                LocalPlayer = Game.LocalPlayer;
                LocalPlayerCharacter = LocalPlayer.Character;

                //hose.Update();

                //if (Game.IsKeyDown(System.Windows.Forms.Keys.H))
                //{
                //    World.SpawnExplosion(LocalPlayerCharacter.GetOffsetPositionFront(8f), 3, 10f, true, false, 0.0f);
                //}

                // if (Game.IsKeyDown(System.Windows.Forms.Keys.U))
                //{
                //    Game.LogTrivial("NumberOfFires: " + World.NumberOfFires);
                //    Fire[] fires = World.GetAllFires();
                //    Game.LogTrivial("GetAllFires.Lenght: " + fires.Length);
                //    for (int i = 0; i < fires.Length; i++)
                //    {
                //        if (fires[i].Exists())
                //            fires[i].Delete();
                //    }

                //    Game.LogTrivial("After NumberOfFires: " + World.NumberOfFires);
                //}


                //if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                //{
                //    Camera cam = new Camera(true);
                //    cam.Position = Game.LocalPlayer.Character.GetOffsetPositionFront(10f);
                //    cam.Active = true;

                //    GameFiber.Sleep(5000);

                //    cam.Delete();
                //}

                FireStationsManager.Instance.Update();
                PlayerManager.Instance.Update();
                PlayerFireEquipmentManager.Instance.Update();
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            FireStationsManager.Instance.CleanUp(isTerminating);
            //PlayerManager.Instance.CleanUp(isTerminating);
            //PlayerEquipmentManager.Instance.CleanUp(isTerminating);

            if (!isTerminating)
            {
                // native calls
            }

            // dispose objects
        }
    }
}
