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
        public int VirtualX = int.MaxValue;
        /// <summary>
        /// computed position
        /// </summary>
        public int VirtualY = int.MaxValue;
        /// <summary>
        /// stored value of our last position before going off-screen
        /// </summary>
        public int LastPositionX;
        // stored value of our last position before going off-screen
        public int LastPositionY;

        public ClientState(string clientName)
        {
            ClientName = clientName;
        }

#if Bail
        public DateTime LastHookEvent_Mouse { get; set; } = DateTime.UtcNow;
        public DateTime LastHookEvent_Keyboard { get; set; } = DateTime.UtcNow;
        public DateTime LastServerEvent_Mouse { get; set; } = DateTime.UtcNow;
        public DateTime LastServerEvent_Keyboard { get; set; } = DateTime.UtcNow; 
#endif

    }
}
