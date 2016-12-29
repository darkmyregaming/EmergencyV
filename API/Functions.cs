namespace EmergencyV.API
{
    // System
    using System;

    // RPH
    using Rage;

    public static class Functions
    {
        public delegate void PlayerDutyStateChangedEventHandler(PlayerStateType currentState, PlayerStateType previousState);
        public delegate void RegisteringCalloutsEventHandler();

        public static event PlayerDutyStateChangedEventHandler PlayerStateChanged;

        /// <summary>
        /// Occurs when the <see cref="Callout"/>s with the <see cref="FireCalloutInfoAttribute"/> are being registered. Use this event to manually register firefighter callouts with <see cref="RegisterFirefighterCallout(Type, string, FirefighterRole, CalloutProbability)"/>.
        /// </summary>
        public static event RegisteringCalloutsEventHandler RegisteringFirefighterCallouts;
        /// <summary>
        /// Occurs when the <see cref="Callout"/>s with the <see cref="EMSCalloutInfoAttribute"/> are being registered. Use this event to manually register firefighter callouts with <see cref="RegisterEMSCallout(Type, string, CalloutProbability)(Type, string, FirefighterRole, CalloutProbability)"/>.
        /// </summary>
        public static event RegisteringCalloutsEventHandler RegisteringEMSCallouts;

        internal static void OnPlayerStateChanged(PlayerStateType currentState, PlayerStateType previousState)
        {
            PlayerStateChanged?.Invoke(currentState, previousState);
        }

        internal static void OnRegisteringCallouts<TCalloutData, TCalloutInfoAttribute>(CalloutsManager<TCalloutData, TCalloutInfoAttribute> calloutsManager) where TCalloutData : RegisteredCalloutData
                                                                                                                                                              where TCalloutInfoAttribute : CalloutInfoAttribute
        {
            if (calloutsManager is FireCalloutsManager)
                RegisteringFirefighterCallouts?.Invoke();
            else if(calloutsManager is EMSCalloutsManager)
                RegisteringEMSCallouts?.Invoke();
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

        /// <summary>
        /// Registers a control mapping. If a key/combo or button/combo conflict occurs, a mapping to an empty keybind will be saved.
        /// </summary>
        /// <param name="name">The unique name of the control mapping.</param>
        /// <param name="control">The control to map to the name.</param>
        /// <returns>false, if the name has already been registered or one of the controls has already been mapped.</returns>
        public static bool RegisterControlMapping(string name, Control control)
        {
            if (Plugin.Controls.ContainsKey(name))
                return false;
            if (IsKeyMapped(control.Key, control.ModifierKey) || IsButtonMapped(control.Button, control.ModifierButton))
            {
                Plugin.Controls[name] = new Control(System.Windows.Forms.Keys.None, ControllerButtons.None);
                Plugin.SaveControls();
                return false;
            }
            Plugin.Controls[name] = control;
            Plugin.SaveControls();
            return true;
        }

        /// <summary>
        /// Either gets or registers a control mapping.
        /// </summary>
        /// <param name="name">The unique name of the control mapping.</param>
        /// <param name="defaultControl">The default control mapping if registering.</param>
        /// <returns>the control mapping, or null, if the registering was unsuccessful.</returns>
        public static Control? GetOrRegisterControlMapping(string name, Control? defaultControl)
        {
            Control? mapping = GetControlMapping(name);
            if (mapping.HasValue)
                return mapping.Value;
            if (!defaultControl.HasValue)
                return null;
            if (!RegisterControlMapping(name, defaultControl.Value))
                return null;
            return defaultControl;
        }

        /// <summary>
        /// Gets a control mapping using its name.
        /// </summary>
        /// <param name="name">The unique name of the control mapping.</param>
        /// <returns>The control mapped to the name, or null.</returns>
        public static Control? GetControlMapping(string name)
        {
            if (!Plugin.Controls.ContainsKey(name))
                return null;
            return Plugin.Controls[name];
        }

        /// <summary>
        /// Removes a control mapping.
        /// </summary>
        /// <param name="name">The name of the control mapping.</param>
        public static void RemoveControlMapping(string name)
        {
            if (!Plugin.Controls.ContainsKey(name))
                return;
            Plugin.Controls.Remove(name);
            Plugin.SaveControls();
        }

        /// <summary>
        /// Checks if a key/key-combo has already been mapped.
        /// </summary>
        /// <param name="key">A key</param>
        /// <param name="modifier">A key modifier</param>
        /// <returns>true, if the key/key-combo has been mapped.</returns>
        public static bool IsKeyMapped(System.Windows.Forms.Keys key, System.Windows.Forms.Keys modifier = System.Windows.Forms.Keys.None)
        {
            return Plugin.Controls.IsKeyMapped(key, modifier);
        }

        /// <summary>
        /// Checks if a controller button/button-combo has already been mapped.
        /// </summary>
        /// <param name="button">A controller button</param>
        /// <param name="modifier">A controller button modifier</param>
        /// <returns>true, if the button/button-combo has been mapped.</returns>
        public static bool IsButtonMapped(ControllerButtons button, ControllerButtons modifier)
        {
            return Plugin.Controls.IsButtonMapped(button, modifier);
        }

        /// <summary>
        /// Returns the result of the CPR attempt on the ped.
        /// </summary>
        /// <param name="ped">The ped to check result.</param>
        /// <returns>null if the ped has not had a CPR attempt. true if the result was successful; otherwise, false.</returns>
        public static bool? WasCPRSuccessful(Ped ped)
        {
            bool result;
            if (!CPR.Instance.TreatedPeds.TryGetValue(ped, out result))
                return null;
            return result;
        }
    }
}
