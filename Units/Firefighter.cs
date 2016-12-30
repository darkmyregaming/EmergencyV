namespace EmergencyV
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class Firefighter
    {
        public Ped Ped { get; }
        public FirefighterEquipmentController Equipment { get; }
        public AIFirefighterController AI { get; }

        public Firefighter(Vector3 position, float heading)
        {
            Ped = new Ped(Plugin.UserSettings.PEDS.FIREFIGHTER_MODEL, position, heading);
            Ped.BlockPermanentEvents = true;

            Equipment = new FirefighterEquipmentController(Ped);
            AI = new AIFirefighterController(this);
        }

        public void Update()
        {
            if (Ped)
            {
                Equipment.Update();
                AI.Update();
            }
        }
    }
}
