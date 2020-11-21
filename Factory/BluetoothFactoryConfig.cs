using System;

namespace RemoteController.Factory
{
    static class BluetoothFactoryConfig
    {
        internal static readonly Type MsftFactoryType = typeof(SocketsBluetoothFactory);
        internal static readonly Type WidcommFactoryType = typeof(WidcommBluetoothFactory);
        internal static readonly Type BlueSoleilFactoryType = typeof(BluesoleilFactory);

        internal static readonly string[] s_knownStacks = {
            WidcommFactoryType.FullName,
            MsftFactoryType.FullName,
            BlueSoleilFactoryType.FullName,
        };

        //----
        internal static string[] KnownStacks
        {
            get
            {
                return s_knownStacks;
            }
        }

        //--------

        // Returns the full path to the running executable on CE (Equivalent to Assembly.GetEntryAssembly() on desktop).
        // Overcomes issue if 32feet .dll is in a different folder to the application (e.g. GAC).
        internal static string GetEntryAssemblyPath()
        {
            System.Reflection.Assembly ea = System.Reflection.Assembly.GetEntryAssembly();
            if (ea == null)
                return null;
            string cb = ea.CodeBase;
            Uri u = new Uri(cb);
            return u.LocalPath;
        }

    }//class
}