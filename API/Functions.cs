namespace EmergencyV.API
{
    // System
    using System;

    // RPH
    using Rage;

    public static class Functions
    {
        public delegate void PlayerDutyStateChangedEventHandler(PlayerStateType currentState, PlayerStateType previousState);

        public static event PlayerDutyStateChangedEventHandler PlayerStateChanged;

        internal static void OnPlayerStateChanged(PlayerStateType currentState, PlayerStateType previousState)
        {
            PlayerStateChanged?.Invoke(currentState, previousState);
        }

        public static ScriptedFire[] CreateFires(Vector3[] positions, int maxChildren, bool isGasFire, bool onGround = true)
        {
            return Util.CreateFires(positions, maxChildren, isGasFire, onGround);
        }

        public static bool IsPlayerFirefighter()
        {
            return PlayerManager.Instance.IsFirefighter;
        }

        public static bool IsPlayerEMS()
        {
            return PlayerManager.Instance.IsEMS;
        }

        public static FirefighterRole GetPlayerFirefighterRole()
        {
            return PlayerManager.Instance.FirefighterRole;
        }

        public static bool IsFirefighterCalloutRunning()
        {
            return FireCalloutsManager.Instance.IsCalloutRunning;
        }

        public static bool IsEMSCalloutRunning()
        {
            return EMSCalloutsManager.Instance.IsCalloutRunning;
        }

        public static bool IsAnyCalloutRunning()
        {
            return IsFirefighterCalloutRunning() || IsEMSCalloutRunning();
        }

        public static void StartFirefighterCallout(Type type)
        {
            FireCalloutsManager.Instance.StartCallout(new FireRegisteredCalloutData(type, "API.StartFirefighterCallout." + type.Name, PlayerManager.Instance.FirefighterRole, CalloutProbability.None));
        }

        public static void StartEMSCallout(Type type)
        {
            EMSCalloutsManager.Instance.StartCallout(new EMSRegisteredCalloutData(type, "API.StartEMSCallout." + type.Name, CalloutProbability.None));
        }

        public static void StopCurrentCallout()
        {
            if (FireCalloutsManager.Instance.IsCalloutRunning)
                FireCalloutsManager.Instance.FinishCurrentCallout();
            if (EMSCalloutsManager.Instance.IsCalloutRunning)
                EMSCalloutsManager.Instance.FinishCurrentCallout();
        }

        public static void RegisterFirefighterCallout(Type calloutType, string name, FirefighterRole role, CalloutProbability probability)
        {
            FireCalloutsManager.Instance.RegisterCallout(new FireRegisteredCalloutData(calloutType, name, role, probability));
        }

        public static void UnregisterFirefighterCallout(string name)
        {
            FireCalloutsManager.Instance.UnregisterCallout(name);
        }

        public static void RegisterEMSCallout(Type calloutType, string name, CalloutProbability probability)
        {
            EMSCalloutsManager.Instance.RegisterCallout(new EMSRegisteredCalloutData(calloutType, name, probability));
        }

        public static void UnregisterEMSCallout(string name)
        {
            EMSCalloutsManager.Instance.UnregisterCallout(name);
        }
    }
}
