namespace RemoteController.Core
{
    public class ClientState
    {
        public bool CurrentClientFocused { get; set; } = false;

        public string ClientName { get; }
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

#if BailClient || BailServer
        public System.DateTime LastHookEvent_Mouse = System.DateTime.UtcNow;
        public System.DateTime LastHookEvent_Keyboard = System.DateTime.UtcNow;
        public System.DateTime LastServerEvent_Mouse = System.DateTime.UtcNow;
        public System.DateTime LastServerEvent_Keyboard = System.DateTime.UtcNow;
#endif

#if DEBUG
        public override string ToString()
        {
            return $"{VirtualX}:{VirtualY}";
        }
#endif

    }
}
