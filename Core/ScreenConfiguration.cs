using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RemoteController.Core
{
    public enum Direction
    {
        None,
        Left,
        Right,
        Top,
        Bottom
    }

    public delegate void ScreenAddedHandler(Direction direction, VirtualScreen screen);

    public delegate void ScreenRemovedHandler(VirtualScreen screen);

    public class ScreenConfiguration
    {
        private readonly ConcurrentDictionary<string, List<VirtualScreen>> screens;

        public IReadOnlyList<VirtualScreen> AllScreen
        {
            get
            {
                List<VirtualScreen> res = new List<VirtualScreen>();
                foreach (var item in screens)
                {
                    res.AddRange(item.Value);
                }
                return res;
            }
        }

        public IReadOnlyList<VirtualScreen> this[string clientId]
        {
            get
            {
                return screens[clientId];
            }
        }

        public event ScreenAddedHandler Added;
        public event ScreenRemovedHandler Removed;

        public ScreenConfiguration()
        {
            screens = new ConcurrentDictionary<string, List<VirtualScreen>>();
        }

        public void AddVirtualScreen(VirtualScreen screen)
        {
            if (!screens.TryGetValue(screen.Client, out var virtualScreens))
            {
                virtualScreens = new List<VirtualScreen>();
                this.screens[screen.Client] = virtualScreens;
            }
            virtualScreens.Add(screen);
        }

        public void AddScreensRight(IEnumerable<VirtualScreen> screens)
        {
            var s = GetFurthestRight();
            foreach (VirtualScreen screen in screens)
            {
                s = AddScreenRight(s, screen);
            }
        }

        public void AddScreensLeft(IEnumerable<VirtualScreen> screens)
        {
            var s = GetFurthestLeft();
            foreach (var screen in screens)
            {
                s = AddScreenLeft(s, screen);
            }
        }

        public void AddScreens(IEnumerable<VirtualScreen> screens)
        {
            foreach (var screen in screens)
            {
                if (!this.screens.TryGetValue(screen.Client, out var virtualScreens))
                {
                    virtualScreens = new List<VirtualScreen>();
                    this.screens.TryAdd(screen.Client, virtualScreens);
                }
                virtualScreens.Add(screen);
            }
        }

        public VirtualScreen AddScreen(int virtualX, int virtualY, VirtualScreen other)
        {
            double newXBottomCorner = virtualX + other.Width - 1;
            double newYBottomCorner = virtualY + other.Height - 1;
            //check and make sure we can add this

            foreach (VirtualScreen s in screens.Values.SelectMany(x => x))
            {
                double existingX = s.X;
                double existingY = s.Y;
                double existingXBottomCorner = s.X + s.Width - 1;
                double existingYBottomCorner = s.Y + s.Height - 1;

                //no overlap of x coords
                if (virtualX > existingXBottomCorner || newXBottomCorner < existingX)
                    continue;

                // If one rectangle is above other
                //screen coordinates have the Y flipped, so we have to adjust this part by flipping the comparisons from what you would normally see
                if (virtualY > existingYBottomCorner || newYBottomCorner < existingY)
                    continue;

                return null; //this is a coordinate overlap
            }

            //all good, add the new screen
            VirtualScreen newScreen = new VirtualScreen(other.Client, other.Dpi)
            {
                LocalX = other.X,
                LocalY = other.Y,
                X = virtualX,
                Y = virtualY,
                Width = other.Width,
                Height = other.Height
            };

            if (!screens.ContainsKey(other.Client))
            {
                screens.TryAdd(other.Client, new List<VirtualScreen>());
            }
            screens[other.Client].Add(newScreen);
            return newScreen;
        }

        public VirtualScreen AddScreen(int localX, int localY, int virtualX, int virtualY, int width, int height, Win32.Hooks.Dpi dpi, string client)
        {
            double newXBottomCorner = virtualX + width - 1;
            double newYBottomCorner = virtualY + height - 1;
            //check and make sure we can add this

            foreach (VirtualScreen s in screens.Values.SelectMany(x => x))
            {
                double existingX = s.X;
                double existingY = s.Y;
                double existingXBottomCorner = s.X + s.Width - 1;
                double existingYBottomCorner = s.Y + s.Height - 1;

                //no overlap of x coords
                if (virtualX > existingXBottomCorner || newXBottomCorner < existingX)
                    continue;

                // If one rectangle is above other
                //screen coordinates have the Y flipped, so we have to adjust this part by flipping the comparisons from what you would normally see
                if (virtualY > existingYBottomCorner || newYBottomCorner < existingY)
                    continue;

                return null; //this is a coordinate overlap
            }

            //all good, add the new screen
            VirtualScreen newScreen = new VirtualScreen(client, dpi)
            {
                LocalX = localX,
                LocalY = localY,
                X = virtualX,
                Y = virtualY,
                Width = width,
                Height = height
            };

            if (!screens.ContainsKey(client))
            {
                screens.TryAdd(client, new List<VirtualScreen>());
            }
            screens[client].Add(newScreen);
            return newScreen;
        }

        public VirtualScreen AddScreen(IDisplay display, Win32.Hooks.Dpi dpi, string client)
        {
            return OnScreenChanged(Direction.None, AddScreen(display.X, display.Y, 0, 0, display.Width, display.Height, dpi, client));
        }

        public VirtualScreen AddScreenRight(VirtualScreen orig, VirtualScreen other)
        {
            return OnScreenChanged(Direction.Right, AddScreen(orig.X + orig.Width, orig.Y, other));
        }

        public VirtualScreen AddScreenLeft(VirtualScreen orig, VirtualScreen other)
        {
            return OnScreenChanged(Direction.Left, AddScreen(orig.X - other.Width, orig.Y, other));
        }
        public VirtualScreen AddScreenAbove(VirtualScreen orig, VirtualScreen other)
        {
            return OnScreenChanged(Direction.Top, AddScreen(orig.X, orig.Y - other.Height, other));
        }
        public VirtualScreen AddScreenBelow(VirtualScreen orig, VirtualScreen other)
        {
            return OnScreenChanged(Direction.Bottom, AddScreen(orig.X, orig.Y + orig.Height, other));
        }

        public VirtualScreen AddScreenRight(VirtualScreen orig, IDisplay display, Win32.Hooks.Dpi dpi, string client)
        {
            return OnScreenChanged(Direction.Right, AddScreen(display.X, display.Y, orig.X + orig.Width, orig.Y, display.Width, display.Height, dpi, client));
        }

        public VirtualScreen AddScreenLeft(VirtualScreen orig, IDisplay display, Win32.Hooks.Dpi dpi, string client)
        {
            return OnScreenChanged(Direction.Left, AddScreen(display.X, display.Y, orig.X - display.Width, orig.Y, display.Width, display.Height, dpi, client));
        }
        public VirtualScreen AddScreenAbove(VirtualScreen orig, IDisplay display, Win32.Hooks.Dpi dpi, string client)
        {
            return OnScreenChanged(Direction.Top, AddScreen(display.X, display.Y, orig.X, orig.Y - display.Height, display.Width, display.Height, dpi, client));
        }
        public VirtualScreen AddScreenBelow(VirtualScreen orig, IDisplay display, Win32.Hooks.Dpi dpi, string client)
        {
            return OnScreenChanged(Direction.Bottom, AddScreen(display.X, display.Y, orig.X, orig.Y + orig.Height, display.Width, display.Height, dpi, client));
        }

        public VirtualScreen ValidVirtualCoordinate(int x, int y)
        {
            //Console.WriteLine("checking:"+x+","+y);
            foreach (var client in screens.Values)
            {
                foreach (var s in client)
                {
                    if (x >= s.X && x < (s.X + s.Width) && y >= s.Y && y < (s.Y + s.Height))
                        return s;
                }
            }

            return null;
        }

        public bool Contains(int x, int y)
        {
            //Console.WriteLine("checking:"+x+","+y);
            foreach (var client in screens.Values)
            {
                foreach (var s in client)
                {
                    if (x >= s.X && x < (s.X + s.Width) && y >= s.Y && y < (s.Y + s.Height))
                        return true;
                }
            }

            return false;
        }

        public VirtualScreen GetFurthestRight()
        {
            VirtualScreen furthestRight = VirtualScreen.Empty;
            double maxX = double.MinValue;
            foreach (VirtualScreen s in screens.Values.SelectMany(x => x))
            {
                double maxForThisScreen = s.X + s.Width;
                if (maxForThisScreen > maxX)
                {
                    maxX = maxForThisScreen;
                    furthestRight = s;
                }
            }

            return furthestRight;

        }

        public VirtualScreen GetFurthestLeft()
        {
            VirtualScreen furthestLeft = VirtualScreen.Empty;
            double minX = double.MaxValue;
            foreach (VirtualScreen s in screens.Values.SelectMany(x => x))
            {
                double minForThisScreen = s.X;
                if (minForThisScreen < minX)
                {
                    minX = minForThisScreen;
                    furthestLeft = s;
                }
            }

            return furthestLeft;

        }


        //function to support removing screen in an arbitrary place. Will collapse other screens in.
        public void Remove(VirtualScreen s)
        {
            VirtualScreen left = GetFurthestLeft();
            VirtualScreen right = GetFurthestRight();

            //Screens
            screens[s.Client].Remove(s);

            //so, right now i'm just adding screens left and right. I haven't done much with positioning up and down.
            //i'm going to keep this simple, but eventually we'll want to implement some kind of grid collapsing function
            //like masonry 


            if (s == left || s == right)
                return;

            //for every screen with a starting X coord that's greater than this, move it back towards 0
            foreach (VirtualScreen screen in screens.Values.SelectMany(x => x).ToList())
            {
                if (screen.X > s.X)
                {
                    screen.X -= s.Width;
                }
            }

        }

        //function to support removing screen in an arbitrary place. Will collapse other screens in.
        public bool Remove(string clientId)
        {
            if (screens.TryRemove(clientId, out List<VirtualScreen> removedScreens))
            {
                foreach (var s in removedScreens)
                {

                    VirtualScreen left = GetFurthestLeft();
                    VirtualScreen right = GetFurthestRight();

                    //Screens

                    //so, right now i'm just adding screens left and right. I haven't done much with positioning up and down.
                    //i'm going to keep this simple, but eventually we'll want to implement some kind of grid collapsing function
                    //like masonry 

                    if (s == left || s == right)
                        continue;

                    //for every screen with a starting X coord that's greater than this, move it back towards 0
                    foreach (VirtualScreen screen in screens.Values.SelectMany(x => x).ToList())
                    {
                        if (screen.X > s.X)
                        {
                            screen.X -= s.Width;
                        }
                    }
                    OnScreenRemoved(s);
                }
                return true;
            }
            return false;

        }

        protected VirtualScreen OnScreenChanged(Direction direction, VirtualScreen screen)
        {
            Added?.Invoke(direction, screen);
            return screen;
        }

        protected void OnScreenRemoved(VirtualScreen screen)
        {
            Removed?.Invoke(screen);
        }
    }
}