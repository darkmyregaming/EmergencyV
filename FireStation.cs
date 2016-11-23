namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class FireStation
    {
        public delegate void PlayerEnterFireStationEventHandler(FireStation station);

        public readonly FireStationData Data;

        public Vehicle Firetruck;
        public event PlayerEnterFireStationEventHandler PlayerEntered;

        public FireStation(FireStationData data)
        {
            Data = data;
        }

        public void Update(bool shouldPlayerEnterStationIfNear = false)
        {
            if (Vector3.DistanceSquared(Data.EntrancePosition, Plugin.LocalPlayerCharacter.Position) < 2.0f * 2.0f)
            {
                Game.DisplayHelp("Press ~INPUT_CONTEXT~ to enter", 20);
                if (shouldPlayerEnterStationIfNear)
                {
                    OnPlayerEntered();
                }
            }

            Util.DrawMarker(0, Data.EntrancePosition, Vector3.Zero, new Rotator(0.0f, 0.0f, 0.0f), new Vector3(1f), Color.FromArgb(150, Color.Yellow), true);
        }

        protected void OnPlayerEntered()
        {
            PlayerEntered?.Invoke(this);
        }
    }
}
