using System;

namespace RemoteController.Core
{
    public class ClientState
    {
        public bool CurrentClientFocused { get; set; } = false;
        public string ClientName { get; }

        public ScreenConfiguration ScreenConfiguration { get; } = new ScreenConfiguration();
        /// <summary>
        /// computed position
        /// </summary>
        public double VirtualX = double.MaxValue;
        /// <summary>
        /// computed position
        /// </summary>
        public double VirtualY = double.MaxValue;
        /// <summary>
        /// stored value of our last position before going off-screen
        /// </summary>
        public double LastPositionX;
        // stored value of our last position before going off-screen
        public double LastPositionY;

        public ClientState(string clientName)
        {
            ClientName = clientName;
        }

        public DateTime LastHookEvent_Mouse { get; set; } = DateTime.UtcNow;
        public DateTime LastHookEvent_Keyboard { get; set; } = DateTime.UtcNow;
        public DateTime LastServerEvent_Mouse { get; set; } = DateTime.UtcNow;
        public DateTime LastServerEvent_Keyboard { get; set; } = DateTime.UtcNow;

    }
}
