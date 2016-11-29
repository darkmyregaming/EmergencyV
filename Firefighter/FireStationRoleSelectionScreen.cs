namespace EmergencyV
{
    // System
    using System;
    using System.Drawing;

    // RPH
    using Rage;
    using Rage.Native;

    // AdvancedUI
    using AdvancedUI;
    using AdvancedUI.Simple;
    using Rectangle = AdvancedUI.Simple.Rectangle;

    internal class FireStationRoleSelectionScreen
    {
        public delegate void RoleSelectedEventHandler(FirefighterRole role);

        public readonly FireStation Station;

        private FirefighterRole[] possibleSelections = { FirefighterRole.Engine, FirefighterRole.Battalion, FirefighterRole.Rescue };

        private FirefighterRole previousSelection = FirefighterRole.Engine;
        private FirefighterRole currentSelection = FirefighterRole.Engine;
        public FirefighterRole CurrentSelection
        {
            get { return currentSelection; }
            set
            {
                if (value == currentSelection)
                    return;
                previousSelection = currentSelection;
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
            cam.Position = loc.Position + Vector3.WorldUp * 13.0f + (loc.Rotation.ToVector() * 4.0f);
            cam.SetRotationPitch(-90.0f);
            cam.SetRotationRoll(loc.Heading);
            cam.Active = true;

            InitUI();

            Game.LocalPlayer.HasControl = false;
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
        }

        public void CleanUp()
        {
            if (cam)
                cam.Delete();
            if (tempCam)
                tempCam.Delete();
        
            if (uiSimpleHost != null)
            {
                uiSimpleHost.Fiber.Abort();
                uiSimpleHost.Elements.Clear();
                uiSimpleHost = null;
            }

            Game.LocalPlayer.HasControl = true;
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

            cam.Position = objectiveLoc.Position + Vector3.WorldUp * 13.0f + (objectiveLoc.Rotation.ToVector() * 4.0f);
            cam.SetRotationPitch(-90.0f);
            cam.SetRotationRoll(objectiveLoc.Heading);

            tempCam.Active = true;

            MoveUISelection(role);
            GameFiber.StartNew(() =>
            {
                interpolating = true;
                NativeFunction.Natives.SetCamActiveWithInterp(cam, tempCam, 850, 1, 1);
                GameFiber.Sleep(850);
                cam.Active = true;
                interpolating = false;
            });
        }


        #region UI
        private SimpleHost uiSimpleHost;

        private Rectangle uiEngineRectangle;
        private Label uiEngineLabel;
        private Rectangle uiBattalionRectangle;
        private Label uiBattalionLabel;
        private Rectangle uiRescueRectangle;
        private Label uiRescueLabel;

        private Rectangle uiSelectionRectangle;

        private void InitUI()
        {
            uiSimpleHost = new SimpleHost();

            const float rectWidth = 200f;
            const float rectHeight = 30f;

            uiEngineRectangle = new Rectangle();
            uiBattalionRectangle = new Rectangle();
            uiRescueRectangle = new Rectangle();

            uiEngineRectangle.Location = AdvancedUI.Util.ConvertToCurrentCoordSystem(new PointF(1920 / 2 - rectWidth * 1.75f, 40));
            uiBattalionRectangle.Location = AdvancedUI.Util.ConvertToCurrentCoordSystem(new PointF(1920 / 2 - rectWidth * 0.5f, 40));
            uiRescueRectangle.Location = AdvancedUI.Util.ConvertToCurrentCoordSystem(new PointF(1920 / 2 + rectWidth * 0.75f, 40));

            uiEngineRectangle.Size = uiBattalionRectangle.Size = uiRescueRectangle.Size = AdvancedUI.Util.ConvertToCurrentCoordSystem(new SizeF(rectWidth, rectHeight));

            uiEngineRectangle.Color = uiBattalionRectangle.Color = uiRescueRectangle.Color = Color.FromArgb(165, 5, 5, 5);

            uiEngineLabel = new Label();
            uiBattalionLabel = new Label();
            uiRescueLabel = new Label();

            uiEngineLabel.Text = FirefighterRole.Engine.ToString();
            uiBattalionLabel.Text = FirefighterRole.Battalion.ToString();
            uiRescueLabel.Text = FirefighterRole.Rescue.ToString();

            uiEngineLabel.Alignment = uiBattalionLabel.Alignment = uiRescueLabel.Alignment = TextAlignment.Center;

            uiEngineLabel.Color = uiBattalionLabel.Color = uiRescueLabel.Color = Color.White;

            uiEngineLabel.Location = new PointF(uiEngineRectangle.Location.X + uiEngineRectangle.Size.Width / 2, uiEngineRectangle.Location.Y);
            uiBattalionLabel.Location = new PointF(uiBattalionRectangle.Location.X + uiBattalionRectangle.Size.Width / 2, uiBattalionRectangle.Location.Y);
            uiRescueLabel.Location = new PointF(uiRescueRectangle.Location.X + uiRescueRectangle.Size.Width / 2, uiRescueRectangle.Location.Y);

            uiSelectionRectangle = new Rectangle();
            uiSelectionRectangle.Color = Color.FromArgb(160, Color.Red);
            uiSelectionRectangle.Size = new SizeF(rectWidth + 10, rectHeight + 10);
            uiSelectionRectangle.Location = new PointF(uiEngineRectangle.Location.X - 5, uiEngineRectangle.Location.Y - 5);

            uiSimpleHost.Elements.Add(uiSelectionRectangle);
            uiSimpleHost.Elements.Add(uiEngineRectangle);
            uiSimpleHost.Elements.Add(uiBattalionRectangle);
            uiSimpleHost.Elements.Add(uiRescueRectangle);
            uiSimpleHost.Elements.Add(uiEngineLabel);
            uiSimpleHost.Elements.Add(uiBattalionLabel);
            uiSimpleHost.Elements.Add(uiRescueLabel);

            uiSimpleHost.Elements.ShowAll();
        }

        private void MoveUISelection(FirefighterRole role)
        {
            GameFiber.StartNew(() => 
            {
                PointF origLoc = uiSelectionRectangle.Location;
                Rectangle objRect = GetUIRectangleForRole(role);
                PointF objLoc = new PointF(objRect.Location.X - 5, objRect.Location.Y - 5);

                float percent = 0.0f;
                while (percent < 1.0f)
                {
                    GameFiber.Sleep(5);

                    float x = MathHelper.Lerp(origLoc.X, objLoc.X, percent);
                    uiSelectionRectangle.Location = new PointF(x, uiSelectionRectangle.Location.Y);

                    percent += 0.0425f;
                }
                uiSelectionRectangle.Location = objLoc;
            });
        }

        private Rectangle GetUIRectangleForRole(FirefighterRole role)
        {
            switch (role)
            {
                default:
                case FirefighterRole.None: return null;
                case FirefighterRole.Engine: return uiEngineRectangle;
                case FirefighterRole.Battalion: return uiBattalionRectangle;
                case FirefighterRole.Rescue: return uiRescueRectangle;
            }
        }
        #endregion
    }
}
