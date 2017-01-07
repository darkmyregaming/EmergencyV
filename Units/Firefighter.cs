namespace EmergencyV
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    public class Firefighter
    {
        public Ped Ped { get; }
        public FirefighterEquipmentController Equipment { get; }
        public AIFirefighterController AI { get; }
        public int? PreferedSeatIndex { get; set; }

        public Firefighter(Vector3 position, float heading)
        {
            Ped = new Ped(Plugin.UserSettings.PEDS.FIREFIGHTER_MODEL, position, heading);
            Ped.BlockPermanentEvents = true;

            Equipment = new FirefighterEquipmentController(Ped);
            AI = new AIFirefighterController(this);

            RegisterFirefighter(this);
        }

        private void Update()
        {
            Equipment.Update();
            AI.Update();
        }


        internal static List<Firefighter> CurrentFirefighters = new List<Firefighter>();
        internal static GameFiber FirefightersUpdateFiber;

        private static void RegisterFirefighter(Firefighter f)
        {
            CurrentFirefighters.Add(f);
            if (FirefightersUpdateFiber == null)
            {
                FirefightersUpdateFiber = GameFiber.StartNew(FirefightersUpdateLoop, "Firefighters Update Loop");
            }
        }

        private static void FirefightersUpdateLoop()
        {
            while (true)
            {
                for (int i = CurrentFirefighters.Count - 1; i >= 0; i--)
                {
                    Firefighter f = CurrentFirefighters[i];

                    if (f.Ped)
                    {
                        f.Update();
                    }
                    else
                    {
                        CurrentFirefighters.RemoveAt(i);
                    }
                }

                GameFiber.Yield();
            }
        }
    }
}
