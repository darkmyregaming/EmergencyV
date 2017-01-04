namespace EmergencyV
{
    // System
    using System.Collections.Generic;

    // RPH
    using Rage;
    using Rage.Native;

    internal class Menu
    {
        public GameFiber Fiber { get; }

        private bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                if (value == isVisible)
                    return;
                isVisible = value;
                if (IsInSubmenu)
                    OpenedSubmenu.IsVisible = value;
            }
        }

        private Menu parentMenu;
        public Menu ParentMenu
        {
            get { return parentMenu; }
            set
            {
                if (value == parentMenu)
                    return;
                parentMenu = value;
                X = parentMenu.X - MenuItem.ItemWidth - 10.0f;
            }
        }

        public List<MenuItem> Items { get; set; }

        private MenuItem selectedItem;
        public MenuItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value == selectedItem)
                    return;
                selectedItem = value;
                SelectedItemIndex = Items.IndexOf(selectedItem);
            }
        }
        public int SelectedItemIndex { get; private set; }

        public float X { get; set; } = (Game.Resolution.Width - MenuItem.ItemWidth - 10.0f);
        public bool IsInSubmenu { get; private set; }
        public Menu OpenedSubmenu { get; private set; }

        public Menu()
        {
            Items = new List<MenuItem>();
            Fiber = GameFiber.StartNew(UpdateLoop, "Menu Fiber");
            Game.RawFrameRender += OnRawFrameRender;
        }

        public void CloseSubmenu()
        {
            if (IsInSubmenu)
            {
                OpenedSubmenu.IsVisible = false;
                OpenedSubmenu = null;
                IsInSubmenu = false;
            }
        }

        public void Dispose()
        {
            Fiber.Abort();
            Game.RawFrameRender -= OnRawFrameRender;
        }

        private void MoveUp()
        {
            if (Items.Count == 0 || SelectedItem == null)
                return;

            int currentIndex = SelectedItemIndex;
            if (currentIndex != -1)
            {
                int nextIndex = currentIndex - 1;
                if (nextIndex >= 0)
                    SelectedItem = Items[nextIndex];
            }
        }

        private void MoveDown()
        {
            if (Items.Count == 0 || SelectedItem == null)
                return;

            int currentIndex = SelectedItemIndex;
            if (currentIndex != -1)
            {
                int nextIndex = currentIndex + 1;
                if (nextIndex < Items.Count)
                    SelectedItem = Items[nextIndex];
            }
        }

        private void UpdateLoop()
        {
            while (true)
            {
                GameFiber.Yield();
                OnUpdate();
            }
        }


        private void OnUpdate()
        {
            if (Game.Console.IsOpen)
                return;

            if (IsVisible)
            {
                if (SelectedItem == null && Items.Count > 0)
                {
                    SelectedItem = Items[0];
                }

                if (!IsInSubmenu)
                {
                    DisableControls();

                    if (Game.IsControlJustPressed(0, GameControl.FrontendUp)) // up
                    {
                        MoveUp();
                    }
                    else if (Game.IsControlJustPressed(0, GameControl.FrontendDown)) // down
                    {
                        MoveDown();
                    }
                    else if (SelectedItem != null && Game.IsControlJustPressed(0, GameControl.FrontendAccept))
                    {
                        if (SelectedItem.BindedSubmenu != null)
                        {
                            GameFiber.Sleep(5); // sleep, otherwise will activate the selected item in the submenu
                            SelectedItem.BindedSubmenu.IsVisible = true;
                            OpenedSubmenu = SelectedItem.BindedSubmenu;
                            IsInSubmenu = true;
                        }

                        SelectedItem?.Callback?.Invoke();
                    }
                    else if (parentMenu != null && Game.IsControlJustPressed(0, GameControl.FrontendRight))
                    {
                        parentMenu.IsInSubmenu = false;
                        OpenedSubmenu = null;
                        IsVisible = false;
                    }
                }
                
            }

            for (int i = 0; i < Items.Count; i++)
            {
                MenuItem item = Items[i];
                if (item.Callback != null && item.ShortcutControl.HasValue && item.ShortcutControl.Value.IsJustPressed())
                {
                    item.Callback.Invoke();
                }
            }
        }

        private void OnRawFrameRender(object sender, GraphicsEventArgs e)
        {
            if (IsVisible)
            {
                Graphics g = e.Graphics;
                for (int i = 0; i < Items.Count; i++)
                {
                    Items[i].OnDraw(g, X, i - SelectedItemIndex);
                }
            }
        }

        private static void DisableControls()
        {
            foreach (GameControl control in System.Enum.GetValues(typeof(GameControl)))
            {
                Game.DisableControlAction(0, control, true);
                //NativeFunction.CallByName<uint>("DISABLE_CONTROL_ACTION", 1, (int)con);
                //NativeFunction.CallByName<uint>("DISABLE_CONTROL_ACTION", 2, (int)con);
            }
            //Controls we want
            // -Frontend
            // -Mouse
            // -Walk/Move
            // -

            foreach (GameControl control in controlsWhitelist)
            {
                NativeFunction.Natives.EnableControlAction(0, (int)control);
            }
        }

        private static List<GameControl> controlsWhitelist = new List<GameControl>
        {
            GameControl.FrontendAccept,
            GameControl.FrontendAxisX,
            GameControl.FrontendAxisY,
            GameControl.FrontendDown,
            GameControl.FrontendUp,
            GameControl.FrontendLeft,
            GameControl.FrontendRight,
            GameControl.FrontendCancel,
            GameControl.FrontendSelect,
            GameControl.CursorScrollDown,
            GameControl.CursorScrollUp,
            GameControl.CursorX,
            GameControl.CursorY,
            GameControl.MoveUpDown,
            GameControl.MoveLeftRight,
            GameControl.Sprint,
            GameControl.Jump,
            GameControl.Enter,
            GameControl.VehicleExit,
            GameControl.VehicleAccelerate,
            GameControl.VehicleBrake,
            GameControl.VehicleMoveLeftRight,
            GameControl.VehicleFlyYawLeft,
            GameControl.ScriptedFlyLeftRight,
            GameControl.ScriptedFlyUpDown,
            GameControl.VehicleFlyYawRight,
            GameControl.VehicleHandbrake,
            GameControl.LookUpDown,
            GameControl.LookLeftRight,
            GameControl.Aim,
            GameControl.Attack,
        };
    }
}
