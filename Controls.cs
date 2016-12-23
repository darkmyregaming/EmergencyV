namespace EmergencyV
{
    // System
    using System;
    using System.IO;
    using System.Xml;
    using System.Windows.Forms;
    using System.Runtime.Serialization;

    // RPH
    using Rage;
    using Rage.Native;

    [DataContract(Name = "ControlsSettings", Namespace = "EmergencyV")]
    internal class ControlsSettings
    {
        [DataMember(Order = 0)]
        public Control ACCEPT_CALLOUT;
        [DataMember(Order = 1)]
        public Control FORCE_CALLOUT;
        [DataMember(Order = 1)]
        public Control END_CALLOUT;


        public static ControlsSettings GetDefault()
        {
            return new ControlsSettings()
            {
                ACCEPT_CALLOUT = new Control()
                {
                    Key = Keys.Y,
                },
                FORCE_CALLOUT = new Control()
                {
                    Key = Keys.X,
                },
                END_CALLOUT = new Control()
                {
                    Key = Keys.End,
                },
            };
        }


        [DataContract(Name = "Control", Namespace = "EmergencyV")]
        public class Control
        {
            private static bool IsUsingController => !NativeFunction.CallByHash<bool>(0xa571d46727e2b718, 2);


            [DataMember(Order = 0)]
            public Keys Key;
            [DataMember(Order = 1, IsRequired = false)]
            public Keys ModifierKey;

            [DataMember(Order = 2)]
            public ControllerButtons Button;
            [DataMember(Order = 3, IsRequired = false)]
            public ControllerButtons ModifierButton;


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
}
