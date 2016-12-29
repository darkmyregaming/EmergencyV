namespace EmergencyV
{
    // System
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Windows.Forms;
    using System.Runtime.Serialization;

    // RPH
    using Rage;
    using Rage.Native;

    [CollectionDataContract
        (Name = "Controls",
        ItemName = "Mapping",
        KeyName = "Name",
        ValueName = "Controls",
        Namespace = "EmergencyV")]
    internal class Controls : Dictionary<string, Control>
    {
        public string GetMappedKey(Keys key, Keys modifier = System.Windows.Forms.Keys.None)
        {
            if (key == System.Windows.Forms.Keys.None)
                return null;
            foreach (KeyValuePair<string, Control> mapping in this)
                if (mapping.Value.Key == key && mapping.Value.ModifierKey == modifier)
                    return mapping.Key;
            return null;
        }

        public string GetMappedButton(ControllerButtons btn, ControllerButtons modifier)
        {
            if (btn == ControllerButtons.None)
                return null;
            foreach (KeyValuePair<string, Control> mapping in this)
                if (mapping.Value.Button == btn && mapping.Value.ModifierButton == modifier)
                    return mapping.Key;
            return null;
        }

        public bool IsKeyMapped(Keys key, Keys modifier = System.Windows.Forms.Keys.None)
        {
            return GetMappedKey(key, modifier) != null;
        }

        public bool IsButtonMapped(ControllerButtons btn, ControllerButtons modifier)
        {
            return GetMappedButton(btn, modifier) != null;
        }

        public static Controls GetDefault()
        {
            return new Controls()
            {
                { "ACCEPT_CALLOUT", new Control(System.Windows.Forms.Keys.Y, ControllerButtons.None) },
                { "FORCE_CALLOUT", new Control(System.Windows.Forms.Keys.X, ControllerButtons.None) },
                { "END_CALLOUT", new Control(System.Windows.Forms.Keys.End, ControllerButtons.None) },
            };
        }
    }

    [DataContract(Namespace = "EmergencyV")]
    public struct Control
    {
        private static bool IsUsingController => !NativeFunction.CallByHash<bool>(0xa571d46727e2b718, 2);

        [DataMember(Order = 1)]
        internal Keys Key;
        [DataMember(Order = 2, IsRequired = false)]
        internal Keys ModifierKey;

        [DataMember(Order = 3)]
        internal ControllerButtons Button;
        [DataMember(Order = 4, IsRequired = false)]
        internal ControllerButtons ModifierButton;

        public Control(Keys key, ControllerButtons button) : this(key, Keys.None, button, ControllerButtons.None) {}

        public Control(Keys key, Keys modKey, ControllerButtons button, ControllerButtons modButton)
        {
            Key = key;
            ModifierKey = modKey;
            Button = button;
            ModifierButton = modButton;
        }

        public bool IsJustPressed()
        {
            if (IsUsingController)
            {
                bool modifierButtonPressed = ModifierButton == ControllerButtons.None ? true : Game.IsControllerButtonDownRightNow(ModifierButton);

                return modifierButtonPressed && (Button == ControllerButtons.None ? false : Game.IsControllerButtonDown(Button));
            }
            else
            {
                bool modifierKeyPressed = ModifierKey == Keys.None ? true : Game.IsKeyDownRightNow(ModifierKey);

                return modifierKeyPressed && (Key == Keys.None ? false : Game.IsKeyDown(Key));
            }
        }

        public bool IsPressed()
        {
            if (IsUsingController)
            {
                bool modifierButtonPressed = ModifierButton == ControllerButtons.None ? true : Game.IsControllerButtonDownRightNow(ModifierButton);

                return modifierButtonPressed && (Button == ControllerButtons.None ? false : Game.IsControllerButtonDownRightNow(Button));
            }
            else
            {
                bool modifierKeyPressed = ModifierKey == Keys.None ? true : Game.IsKeyDownRightNow(ModifierKey);

                return modifierKeyPressed && (Key == Keys.None ? false : Game.IsKeyDownRightNow(Key));
            }
        }
    }

}
