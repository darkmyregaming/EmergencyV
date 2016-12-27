namespace EmergencyV
{
    using System.Collections.Generic;

    using Rage;
    using Rage.Native;

    // NOTE: this is a WIP and not completely tested.
    internal class DeathManager
    {
        private static DeathManager instance;
        public static DeathManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new DeathManager();
                return instance;
            }
        }

        private Dictionary<Ped, string> DeathCause = new Dictionary<Ped, string>();
        private Dictionary<Ped, string> OriginalDeathCause = new Dictionary<Ped, string>();

        public void ShowReport(Ped ped)
        {
            if (!ped || ped.IsAlive)
                return;

            string sex = ped.IsMale ? "Male" : "Female";
            string cause = GetCause(ped);

            string report = sex + '\n' +
                "Cause of Death: " + Util.FirstCharToUpper(cause);

            Notification.Show("Death Report", report, 10.0d);
        }

        public string GetCause(Ped ped)
        {
            if (DeathCause.ContainsKey(ped))
                return DeathCause[ped];

            uint hash = NativeFunction.Natives.GetPedCauseOfDeath<uint>(ped);
            string cause = "unknown";
            if (typeof(WeaponHash).IsEnumDefined(hash))
            {
                var weapon = (WeaponHash) hash;
                cause = Util.EnumNameToDelimitedString(typeof(WeaponHash), weapon, ' ');
            }

            if (!OriginalDeathCause.ContainsKey(ped))
                OriginalDeathCause[ped] = cause;

            DeathCause[ped] = cause;

            return cause;
        }

        public void OverrideCause(Ped ped, string cause)
        {
            GetCause(ped); // set default, if haven't already

            DeathCause[ped] = cause;
        }

        public string ResetToOriginalCause(Ped ped)
        {
            if (!OriginalDeathCause.ContainsKey(ped))
                return GetCause(ped);
            string original = OriginalDeathCause[ped];
            DeathCause[ped] = original;
            return original;
        }
    }
}
