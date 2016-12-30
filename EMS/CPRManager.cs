namespace EmergencyV
{
    using System;
    using System.Collections.Generic;

    using Rage;

    internal class CPRManager
    {
        private static CPRManager instance;
        internal static CPRManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new CPRManager();
                return instance;
            }
        }

        internal Dictionary<Ped, bool> TreatedPeds { get; } = new Dictionary<Ped, bool>();

        internal bool ShouldSearchLocally { get; set; } = true;

        private List<CPR> Active { get; } = new List<CPR>();

        internal void Start(CPR cpr)
        {
            if (Active.Contains(cpr))
                return;
            Active.Add(cpr);
            cpr.Start();
        }

        internal bool HasActiveCPR(Ped patient)
        {
            foreach (CPR cpr in Active)
            {
                if (cpr.Patient == patient)
                    return true;
            }
            return false;
        }

        private void searchLocally()
        {
            Ped localPatient = Util.GetClosestDeadPed(Game.LocalPlayer.Character.Position, 1.75f);

            if (localPatient && !HasActiveCPR(localPatient) && !TreatedPeds.ContainsKey(localPatient))
            {
                Game.DisplayHelp("Press ~INPUT_CONTEXT~ to attempt CPR", 20);

                if (Game.IsControlJustPressed(0, GameControl.Context))
                {
                    Start(new CPR(localPatient, Game.LocalPlayer.Character));
                }
            }
        }

        internal void Update()
        {
            if (ShouldSearchLocally)
                searchLocally();

            if (Active.Count > 0)
            {
                foreach (CPR cpr in Active)
                {
                    cpr.Update();

                    if (cpr.IsFinished)
                        TreatedPeds.Add(cpr.Patient, cpr.WasSuccessful);
                }
                Active.RemoveAll(cpr => cpr.IsFinished);
            }
        }
    }
}