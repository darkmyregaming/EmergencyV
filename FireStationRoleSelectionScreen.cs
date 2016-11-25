namespace EmergencyV
{
    // System
    using System;

    // RPH
    using Rage;
    using Rage.Native;

    internal class FireStationRoleSelectionScreen
    {
        public delegate void RoleSelectedEventHandler(FirefighterRole role);

        public readonly FireStation Station;

        private FirefighterRole[] possibleSelections = { FirefighterRole.Engine, FirefighterRole.Battalion, FirefighterRole.Rescue };

        private FirefighterRole currentSelection = FirefighterRole.Engine;
        public FirefighterRole CurrentSelection
        {
            get { return currentSelection; }
            set
            {
                if (value == currentSelection)
                    return;
                currentSelection = value;
                MoveCamForRole(currentSelection);
                Game.LogTrivial("Current selection is " + currentSelection);
            }
        }

        public event RoleSelectedEventHandler RoleSelected;

        private Camera cam;
        private Camera tempCam;
        private bool interpolating;

        public FireStationRoleSelectionScreen(FireStation station)
        {
            Game.LogTrivial("Entered role selection screen in station " + station.Data.Name);
            Station = station;

            RotatedVector3 loc = Station.GetVehicleLocationForRole(FirefighterRole.Engine);

            tempCam = new Camera(false);

            cam = new Camera(true);
            cam.Position = loc.Position + Vector3.WorldUp * 12.0f;
            cam.SetRotationPitch(-90.0f);
            cam.SetRotationRoll(loc.Heading);
            cam.Active = true;
        }

        public void Update()
        {
            if (!interpolating)
            {
                if (Game.IsKeyDown(System.Windows.Forms.Keys.Left))
                {
                    int newSelectionIndex = Array.IndexOf(possibleSelections, currentSelection) - 1;
                    if (newSelectionIndex < 0)
                        newSelectionIndex = possibleSelections.Length - 1;
                    CurrentSelection = possibleSelections[newSelectionIndex];
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.Right))
                {
                    int newSelectionIndex = Array.IndexOf(possibleSelections, currentSelection) + 1;
                    if (newSelectionIndex >= possibleSelections.Length)
                        newSelectionIndex = 0;
                    CurrentSelection = possibleSelections[newSelectionIndex];
                }
                else if (Game.IsKeyDown(System.Windows.Forms.Keys.Enter))
                {
                    OnRoleSelected(CurrentSelection);
                }
            }
            
            Game.DisplaySubtitle($"Current Selection~n~~o~{CurrentSelection}", 25);
        }

        public void CleanUp()
        {
            if (cam)
                cam.Delete();
            if (tempCam)
                tempCam.Delete();
        }

        protected void OnRoleSelected(FirefighterRole role)
        {
            RoleSelected?.Invoke(role);
        }

        private void MoveCamForRole(FirefighterRole role)
        {
            RotatedVector3 objectiveLoc = Station.GetVehicleLocationForRole(role);

            tempCam.Position = cam.Position;
            tempCam.Rotation = cam.Rotation;

            cam.Position = objectiveLoc.Position + Vector3.WorldUp * 12.0f;
            cam.SetRotationPitch(-90.0f);
            cam.SetRotationRoll(objectiveLoc.Heading);

            tempCam.Active = true;

            GameFiber.StartNew(() =>
            {
                interpolating = true;
                NativeFunction.Natives.SetCamActiveWithInterp(cam, tempCam, 2000, 1, 1);
                GameFiber.Sleep(2000);
                cam.Active = true;
                interpolating = false;
            });
        }
    }
}
