using RemoteController.Core;
using RemoteController.Win32.Hooks;
using System;
using System.Linq;

namespace RemoteController.Desktop
{
    public enum CoordinateCalculationResult
    {
        Valid,
        Invalid
    }

    public struct CoordinateUpdateResult
    {
        public bool MoveMouse { get; set; }
        public bool HandleEvent { get; set; }
    }

    public class VirtualScreenManager
    {

        public readonly ClientState State;

        public VirtualScreenManager(ClientState clientState)
        {
            State = clientState;
        }

        public CoordinateCalculationResult UpdateVirtualMouseCoordinates(MouseMoveEventArgs e)
        {

            //calculate the change from previous stored coordinates
            double deltaX = e.Mouse.X - State.LastPositionX;
            double deltaY = e.Mouse.Y - State.LastPositionY;


            //Console.WriteLine(deltaY+": " + e.Mouse.Y + " - " + ClientState._lastPositionY);

            State.VirtualX += deltaX;
            State.VirtualY += deltaY;

            VirtualScreen s = State.ScreenConfiguration.ValidVirtualCoordinate(State.VirtualX, State.VirtualY);
            //Console.WriteLine("hook " + e.Mouse.X + "," + e.Mouse.Y + " : delta " + deltaX + "," + deltaY + " : virtual " + ClientState._virtualX + ", " + ClientState._virtualY + ", lastpos:"+ClientState._lastPositionX+","+ClientState._lastPositionY);
            if (s != null)
            {
                return CoordinateCalculationResult.Valid;
            }

            State.VirtualX -= deltaX;
            State.VirtualY -= deltaY;

            return CoordinateCalculationResult.Invalid;

        }


        //TODO: rewrite this. there are really only  3 outcomes of this function.
        // 1) hide the mouse, show the mouse, replay the mouse from a remote server
        // 2) translate virtual coords to screen coords.
        // 2) update a field tracking the last local position of the mouse  -WHY DON'T I JUST ASK THE OS????

        //based on the current position of the virtual coordinates
        //decide whether to hide the mouse, pass the coords to the hook, or handle the event, or some combo.
        public CoordinateUpdateResult ProcessVirtualCoordinatesUpdate()
        {

            CoordinateUpdateResult result = new CoordinateUpdateResult();

            //find the current screen
            //these coordinates may have just been updated from the server
            VirtualScreen s = State.ScreenConfiguration.ValidVirtualCoordinate(State.VirtualX, State.VirtualY);
            if (s == null)
                return result;

            if (s.Client == State.ClientName)
            {
                State.LastPositionX = Math.Abs(State.VirtualX - s.X) + s.LocalX;
                State.LastPositionY = Math.Abs(State.VirtualY - s.Y) + s.LocalY;

                //we previous weren't focused, but now we are
                if (!State.CurrentClientFocused)
                {
                    //_hook.Hook.SetMousePos((int)localX, (int)localY);
                    result.MoveMouse = true;
                    result.HandleEvent = true;
                    //Console.WriteLine("regaining focus: " + localX + ","+ localY);

                    //mark this as handled since we manually set the cursor and don't want it to rubberband around
                }
                State.CurrentClientFocused = true;

            }
            else
            {
                if (State.CurrentClientFocused)
                {
                    //we have moved off screen now - hide the mouse                    
                    VirtualScreen screen = State.ScreenConfiguration.Screens[State.ClientName].First();

                    //hide mouse
                    //Console.WriteLine("detected coordinates outside of our current screen - hiding mouse at " + localMaxX + "," + localMaxY);
                    //_hook.Hook.SetMousePos((int)localMaxX, (int)localMaxY);
                    result.MoveMouse = true;
                    State.LastPositionX = (int)(0 + screen.Width - 1);
                    State.LastPositionY = (int)(0 + screen.Height - 1);
                    //Console.WriteLine("Setting last known position of mouse to " + localMaxX + "," + localMaxY);
                }

                //we are offscreen
                State.CurrentClientFocused = false;
                result.HandleEvent = true;
            }

            return result;
        }
    }
}
