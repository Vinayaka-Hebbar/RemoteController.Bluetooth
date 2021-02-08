using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RemoteController.Core
{
    public class ScreenConfiguration
    {
        public ConcurrentDictionary<string, List<VirtualScreen>> Screens { get; }

        public IList<VirtualScreen> AllScreen
        {
            get
            {
                List<VirtualScreen> res = new List<VirtualScreen>();
                foreach (var item in Screens)
                {
                    res.AddRange(item.Value);
                }
                return res;
            }
        }

        public ScreenConfiguration()
        {
            Screens = new ConcurrentDictionary<string, List<VirtualScreen>>();
        }

        public void AddScreensRight(IList<VirtualScreen> screens)
        {
            foreach (var screen in screens)
            {
                //Console.WriteLine("Screen:"+screen.X+","+screen.Y + ", LocalX:"+screen.LocalX + ", "+screen.LocalY + " , Width:"+screen.Width + " , height:"+screen.Height+", client: "+ screen.Client);
                if (!Screens.ContainsKey(screen.Client))
                {
                    Screens.TryAdd(screen.Client, new List<VirtualScreen>());
                    AddScreenRight(GetFurthestLeft(), screen);
                }
            }
        }

        public void AddScreensLeft(IList<VirtualScreen> screens)
        {
            foreach (var screen in screens)
            {
                //Console.WriteLine("Screen:"+screen.X+","+screen.Y + ", LocalX:"+screen.LocalX + ", "+screen.LocalY + " , Width:"+screen.Width + " , height:"+screen.Height+", client: "+ screen.Client);
                if (!Screens.ContainsKey(screen.Client))
                {
                    Screens.TryAdd(screen.Client, new List<VirtualScreen>());
                    AddScreenRight(GetFurthestLeft(), screen);
                }
            }
        }

        public VirtualScreen AddScreen(int virtualX, int virtualY, VirtualScreen other)
        {
            double newXBottomCorner = virtualX + other.Width - 1;
            double newYBottomCorner = virtualY + other.Height - 1;
            //check and make sure we can add this

            foreach (VirtualScreen s in Screens.Values.SelectMany(x => x))
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

            if (!Screens.ContainsKey(other.Client))
            {
                Screens.TryAdd(other.Client, new List<VirtualScreen>());

            }
            Screens[other.Client].Add(newScreen);


            return newScreen;
        }

        public VirtualScreen AddScreen(int localX, int localY, int virtualX, int virtualY, int width, int height, Win32.Hooks.Dpi dpi, string client)
        {
            double newXBottomCorner = virtualX + width - 1;
            double newYBottomCorner = virtualY + height - 1;
            //check and make sure we can add this

            foreach (VirtualScreen s in Screens.Values.SelectMany(x => x))
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

            if (!Screens.ContainsKey(client))
            {
                Screens.TryAdd(client, new List<VirtualScreen>());

            }
            Screens[client].Add(newScreen);


            return newScreen;
        }

        public VirtualScreen AddScreenRight(VirtualScreen orig, VirtualScreen other)
        {
            return AddScreen(orig.X + orig.Width, orig.Y, other);
        }
        public VirtualScreen AddScreenLeft(VirtualScreen orig, VirtualScreen other)
        {
            return AddScreen(orig.X - other.Width, orig.Y, other);
        }
        public VirtualScreen AddScreenAbove(VirtualScreen orig, VirtualScreen other)
        {
            return AddScreen(orig.X, orig.Y - other.Height, other);
        }
        public VirtualScreen AddScreenBelow(VirtualScreen orig,  VirtualScreen other)
        {
            return AddScreen(orig.X, orig.Y + orig.Height, other);
        }

        public VirtualScreen ValidVirtualCoordinate(int x, int y)
        {
            //Console.WriteLine("checking:"+x+","+y);
            foreach (var client in Screens.Values)
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
            foreach (var client in Screens.Values)
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
            VirtualScreen furthestRight = null;
            double maxX = double.MinValue;
            foreach (VirtualScreen s in Screens.Values.SelectMany(x => x))
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
            foreach (VirtualScreen s in Screens.Values.SelectMany(x => x))
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
            Screens[s.Client].Remove(s);

            //so, right now i'm just adding screens left and right. I haven't done much with positioning up and down.
            //i'm going to keep this simple, but eventually we'll want to implement some kind of grid collapsing function
            //like masonry 


            if (s == left || s == right)
                return;

            //for every screen with a starting X coord that's greater than this, move it back towards 0
            foreach (VirtualScreen screen in Screens.Values.SelectMany(x => x).ToList())
            {
                if (screen.X > s.X)
                {
                    screen.X -= s.Width;
                }
            }

        }
    }
}