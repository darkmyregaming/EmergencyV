namespace EmergencyV
{
    // System
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Windows.Forms;

    // RPH
    using Rage;
    using Rage.Native;
    
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
                { "OPEN_MENU", new Control(System.Windows.Forms.Keys.Z, ControllerButtons.None) },
                { "TOGGLE_FLASHLIGHT", new Control(System.Windows.Forms.Keys.L, ControllerButtons.None) },
            };
        }
    }
    
    public struct Control
    {
        private static bool IsUsingController => !NativeFunction.CallByHash<bool>(0xa571d46727e2b718, 2);

        [XmlAttribute]
        public Keys Key { get; internal set; }
        [XmlAttribute]
        public Keys ModifierKey { get; internal set; }

        [XmlAttribute]
        public ControllerButtons Button { get; internal set; }
        [XmlAttribute]
        public ControllerButtons ModifierButton { get; internal set; }

        public Control(Keys key, ControllerButtons button) : this(key, Keys.None, button, ControllerButtons.None) {}

        public Control(Keys key, Keys modifierKey, ControllerButtons button, ControllerButtons modifierButton)
        {
            Key = key;
            ModifierKey = modifierKey;
            Button = button;
            ModifierButton = modifierButton;
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

        public string GetDisplayText()
        {
            bool usingController = IsUsingController;

            string modifierText = usingController ?
                                    ModifierButton == ControllerButtons.None ? "" : $"{ModifierButton} + " :
                                    ModifierKey == Keys.None ? "" : $"{ModifierKey} + ";

            string keyText = usingController ?
                                    Button == ControllerButtons.None ? "" : $"{Button}" :
                                    Key == Keys.None ? "" : $"{Key}";

            return modifierText + keyText;
        }
    }

}
