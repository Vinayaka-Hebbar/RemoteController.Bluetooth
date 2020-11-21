using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RemoteController.Core
{
    public class ScreenConfiguration
    {
        public ConcurrentDictionary<string, List<VirtualScreen>> Screens { get; set; }

        public ScreenConfiguration()
        {
            Screens = new ConcurrentDictionary<string, List<VirtualScreen>>();
        }

        public VirtualScreen AddScreen(double localX, double localY, double virtualX, double virtualY, double width, double height, string client)
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
            VirtualScreen newScreen = new VirtualScreen(client)
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

        public VirtualScreen AddScreenRight(VirtualScreen orig, double localX, double localY, double width, double height, string client)
        {
            return AddScreen(localX, localY, orig.X + orig.Width, orig.Y, width, height, client);
        }
        public VirtualScreen AddScreenLeft(VirtualScreen orig, double localX, double localY, double width, double height, string client)
        {
            return AddScreen(localX, localY, orig.X - width, orig.Y, width, height, client);
        }
        public VirtualScreen AddScreenAbove(VirtualScreen orig, double localX, double localY, double width, double height, string client)
        {
            return AddScreen(localX, localY, orig.X, orig.Y - height, width, height, client);
        }
        public VirtualScreen AddScreenBelow(VirtualScreen orig, double localX, double localY, double width, double height, string client)
        {
            return AddScreen(localX, localY, orig.X, orig.Y + orig.Height, width, height, client);
        }

        public VirtualScreen ValidVirtualCoordinate(double x, double y)
        {
            //Console.WriteLine("checking:"+x+","+y);
            foreach (VirtualScreen s in Screens.Values.SelectMany(s => s))
            {
                if (x >= s.X && x < (s.X + s.Width) && y >= s.Y && y < (s.Y + s.Height))
                    return s;
            }

            return null;
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
            VirtualScreen furthestLeft = null;
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