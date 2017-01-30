namespace EmergencyV
{
    // RPH
    using Rage;
    using Rage.Native;

    internal static class EntityCreator
    {
        public static Vehicle CreateFirefighterVehicle(Vector3 position, float heading, FirefighterRole role)
        {
            Model model;
            Settings.ColorData primaryColor, secondaryColor;

            switch (role)
            {
                default:
                case FirefighterRole.None:
                    return null;

                case FirefighterRole.Engine:
                    {
                        model = Plugin.UserSettings.VEHICLES.ENGINE_MODEL;
                        primaryColor = Plugin.UserSettings.VEHICLES.ENGINE_PRIMARY_COLOR;
                        secondaryColor = Plugin.UserSettings.VEHICLES.ENGINE_SECONDARY_COLOR;
                    }
                    break;

                case FirefighterRole.Battalion:
                    {
                        model = Plugin.UserSettings.VEHICLES.BATTALION_MODEL;
                        primaryColor = Plugin.UserSettings.VEHICLES.BATTALION_PRIMARY_COLOR;
                        secondaryColor = Plugin.UserSettings.VEHICLES.BATTALION_SECONDARY_COLOR;
                    }
                    break;

                case FirefighterRole.Rescue:
                    {
                        model = Plugin.UserSettings.VEHICLES.RESCUE_MODEL;
                        primaryColor = Plugin.UserSettings.VEHICLES.RESCUE_PRIMARY_COLOR;
                        secondaryColor = Plugin.UserSettings.VEHICLES.RESCUE_SECONDARY_COLOR;
                    }
                    break;
                    
            }

            Vehicle veh = new Vehicle(model, position, heading);
            if (primaryColor != null) veh.PrimaryColor = primaryColor.ToColor();
            if (secondaryColor != null) veh.SecondaryColor = secondaryColor.ToColor();

            return veh;
        }

        public static Vehicle CreateEMSVehicle(Vector3 position, float heading)
        {
            Model model = Plugin.UserSettings.VEHICLES.AMBULANCE_MODEL;
            Settings.ColorData primaryColor = Plugin.UserSettings.VEHICLES.AMBULANCE_PRIMARY_COLOR;
            Settings.ColorData secondaryColor = Plugin.UserSettings.VEHICLES.AMBULANCE_PRIMARY_COLOR;

            Vehicle veh = new Vehicle(model, position, heading);
            if (primaryColor != null) veh.PrimaryColor = primaryColor.ToColor();
            if (secondaryColor != null) veh.SecondaryColor = secondaryColor.ToColor();

            return veh;
        }


        public static Ped CreateFirefighterPed(Vector3 position, float heading) // TODO: add support for SUP custom peds
        {
            return new Ped(Plugin.UserSettings.PEDS.FIREFIGHTER_MODEL, position, heading);
        }

        public static Ped CreateEMSPed(Vector3 position, float heading)
        {
            return new Ped(Plugin.UserSettings.PEDS.EMS_MODEL, position, heading);
        }
    }
}
