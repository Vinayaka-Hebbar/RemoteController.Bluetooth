using RemoteController.Core;
using RemoteController.Win32.Hooks;
using System;
using System.Linq;

namespace RemoteController.Desktop
{
    public struct CoordinateUpdateResult
    {
        public static readonly CoordinateUpdateResult Empty = default;

        public static readonly CoordinateUpdateResult ValidCoordinate = new CoordinateUpdateResult(false);

        public static readonly CoordinateUpdateResult ValidMouseMove = new CoordinateUpdateResult(true);

        public CoordinateUpdateResult(bool moveMouse)
        {
            MoveMouse = moveMouse;
            IsValid = true;
        }

        public bool MoveMouse { get; }
        public bool IsValid { get; }
    }

    public class VirtualScreenManager
    {
        public readonly ClientState State;



        public ScreenConfiguration ScreenConfiguration { get; }

        public VirtualScreenManager(ClientState clientState)
        {
            State = clientState;
            ScreenConfiguration = new ScreenConfiguration();
        }

        //TODO: rewrite this. there are really only  3 outcomes of this function.
        // 1) hide the mouse, show the mouse, replay the mouse from a remote server
        // 2) translate virtual coords to screen coords.
        // 2) update a field tracking the last local position of the mouse  -WHY DON'T I JUST ASK THE OS????

        //based on the current position of the virtual coordinates
        //decide whether to hide the mouse, pass the coords to the hook, or handle the event, or some combo.
        public CoordinateUpdateResult UpdateVirtualMouseCoordinates(MouseMoveEventArgs e)
        {
            int deltaX, deltaY, newX, newY;
            //calculate the change from previous stored coordinates
            newX = State.VirtualX + (deltaX = e.Mouse.X - State.LastPositionX);
            newY = State.VirtualY + (deltaY = e.Mouse.Y - State.LastPositionY);

            System.Diagnostics.Debug.WriteLine(State);
            var s = ScreenConfiguration.ValidVirtualCoordinate(newX, newY);
            //Console.WriteLine("hook " + e.Mouse.X + "," + e.Mouse.Y + " : delta " + deltaX + "," + deltaY + " : virtual " + ClientState._virtualX + ", " + ClientState._virtualY + ", lastpos:"+ClientState._lastPositionX+","+ClientState._lastPositionY);
            if (s == null)
            {
                e.Handled = !State.CurrentClientFocused;
                return CoordinateUpdateResult.Empty;
            }
            else
            {
                if (s.Client == State.ClientName)
                {
                    State.VirtualX = newX;
                    State.VirtualY = newY;
                    State.LastPositionX = Math.Abs(newX - s.X) + s.LocalX;
                    State.LastPositionY = Math.Abs(newY - s.Y) + s.LocalY;

                    //we previous weren't focused, but now we are
                    if (!State.CurrentClientFocused)
                    {
                        e.Handled = true;
                        State.CurrentClientFocused = true;
                        return CoordinateUpdateResult.ValidMouseMove;
                        //Console.WriteLine("regaining focus: " + localX + ","+ localY);

                        //mark this as handled since we manually set the cursor and don't want it to rubberband around
                    }
                    return CoordinateUpdateResult.ValidCoordinate;
                }
                else if (State.CurrentClientFocused)
                {
                    State.VirtualX = newX;
                    State.VirtualY = newY;
                    //we have moved off screen now - hide the mouse                    
                    VirtualScreen screen = ScreenConfiguration.Screens[State.ClientName].First();

                    //hide mouse
                    //Console.WriteLine("detected coordinates outside of our current screen - hiding mouse at " + localMaxX + "," + localMaxY);
                    //_hook.Hook.SetMousePos((int)localMaxX, (int)localMaxY);
                    State.LastPositionX = screen.Width;
                    State.LastPositionY = screen.Height;
                    //Console.WriteLine("Setting last known position of mouse to " + localMaxX + "," + localMaxY);

                    //we are offscreen
                    State.CurrentClientFocused = false;
                    e.Handled = true;
                    return CoordinateUpdateResult.ValidMouseMove;
                }
                else
                {
                    State.VirtualX += deltaX * s.ScaleX;
                    State.VirtualY += deltaY * s.ScaleY;
                    e.Handled = true;
                    return CoordinateUpdateResult.ValidCoordinate;
                }

            }
        }

        public bool ProcessVirtualCoordinatesMove(bool replay)
        {
            //find the current screen
            //these coordinates may have just been updated from the server
            VirtualScreen s = ScreenConfiguration.ValidVirtualCoordinate(State.VirtualX, State.VirtualY);
            if (s == null)
                return false;
            bool result = false;
            if (s.Client == State.ClientName)
            {
                State.LastPositionX = Math.Abs(State.VirtualX - s.X) + s.LocalX;
                State.LastPositionY = Math.Abs(State.VirtualY - s.Y) + s.LocalY;

                //we previous weren't focused, but now we are
                if (!State.CurrentClientFocused)
                {
                    result = true;
                    State.CurrentClientFocused = true;
                    return result;
                    //Console.WriteLine("regaining focus: " + localX + ","+ localY);

                    //mark this as handled since we manually set the cursor and don't want it to rubberband around
                }
                result = replay;

            }
            else
            {
                if (State.CurrentClientFocused)
                {
                    //we have moved off screen now - hide the mouse                    
                    VirtualScreen screen = ScreenConfiguration.Screens[State.ClientName].First();

                    //hide mouse
                    //Console.WriteLine("detected coordinates outside of our current screen - hiding mouse at " + localMaxX + "," + localMaxY);
                    //_hook.Hook.SetMousePos((int)localMaxX, (int)localMaxY);
                    result = true;
                    State.LastPositionX = (int)(0 + screen.Width - 1);
                    State.LastPositionY = (int)(0 + screen.Height - 1);
                    //Console.WriteLine("Setting last known position of mouse to " + localMaxX + "," + localMaxY);
                }

                //we are offscreen
                State.CurrentClientFocused = false;
            }

            return result;
        }
    }
}
