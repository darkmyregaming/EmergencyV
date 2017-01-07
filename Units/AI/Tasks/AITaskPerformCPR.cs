namespace EmergencyV
{
    // RPH
    using Rage;


    internal class AITaskPerformCPR : AITask
    {
        CPR cpr;

        protected AITaskPerformCPR(Ped ped, Ped patient) : base(ped)
        {
            cpr = new CPR(patient, ped);
            CPRManager.Instance.Start(cpr);
        }

        internal override void Update()
        {
            if (!cpr.IsPerforming)
                IsFinished = true;
        }

        protected override void OnFinished()
        {
        }
    }
}
