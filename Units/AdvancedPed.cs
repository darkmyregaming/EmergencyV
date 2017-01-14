namespace EmergencyV
{
    using System;
    // System
    using System.Collections.Generic;

    // RPH
    using Rage;

    public abstract class AdvancedPed
    {
        public Ped Ped { get; }
        public int? PreferedSeatIndex { get; set; }

        internal AdvancedPed(Model model, Vector3 position, float heading)
        {
            Ped = new Ped(model, position, heading);
            Ped.BlockPermanentEvents = true;

            RegisterAdvancedPed(this);
        }

        protected abstract void Update();


        internal static List<AdvancedPed> CurrentAdvancedPeds = new List<AdvancedPed>();
        internal static GameFiber AdvancedPedsUpdateFiber;

        private static void RegisterAdvancedPed(AdvancedPed p)
        {
            CurrentAdvancedPeds.Add(p);
            if (AdvancedPedsUpdateFiber == null)
            {
                AdvancedPedsUpdateFiber = GameFiber.StartNew(AdvancedPedsUpdateLoop, "AdvancedPeds Update Loop");
            }
        }

        private static void AdvancedPedsUpdateLoop()
        {
            while (true)
            {
                for (int i = CurrentAdvancedPeds.Count - 1; i >= 0; i--)
                {
                    AdvancedPed p = CurrentAdvancedPeds[i];

                    if (p.Ped)
                    {
                        p.Update();
                    }
                    else
                    {
                        CurrentAdvancedPeds.RemoveAt(i);
                    }
                }

                GameFiber.Yield();
            }
        }
    }

    public abstract class AdvancedPed<TAIController> : AdvancedPed
                                               where TAIController : AIController
    {
        public TAIController AI { get; }

        internal AdvancedPed(Model model, Vector3 position, float heading) : base(model, position, heading)
        {
            AI = CreateAIController();
        }

        protected override void Update()
        {
            AI?.Update();
        }

        protected abstract TAIController CreateAIController();
    }
}
