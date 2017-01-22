namespace EmergencyV
{
    // RPH
    using Rage;


    internal class AITaskPerformCPR : AITask
    {
        CPR cpr;

        protected AITaskPerformCPR(AIController controller, Ped patient) : base(controller)
        {
            cpr = new CPR(patient, Ped);
            CPRManager.Instance.Start(cpr);
        }

        internal override void Update()
        {
            if (!cpr.IsPerforming)
                IsFinished = true;
        }

        protected override void OnFinished(bool isAborted)
        {
            // TODO: abort CPR in AITaskPerformCPR
        }
    }
}
