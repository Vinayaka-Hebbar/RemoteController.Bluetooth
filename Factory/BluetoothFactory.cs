using RemoteController.Bluetooth;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RemoteController.Factory
{
    public abstract class BluetoothFactory : IDisposable
    {
        protected abstract BluetoothClient GetBluetoothClient();
        protected abstract BluetoothClient GetBluetoothClient(System.Net.Sockets.Socket acceptedSocket);
        protected abstract BluetoothClient GetBluetoothClient(BluetoothEndPoint localEP);
        protected abstract BluetoothDeviceInfo GetBluetoothDeviceInfo(BluetoothAddress address);

        protected abstract BluetoothRadio GetPrimaryRadio();
        protected abstract BluetoothRadio[] GetAllRadios();

        protected abstract BluetoothSecurity GetBluetoothSecurity();
        //
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected abstract void Dispose(bool disposing);

        //--------------------------------------------------------------
        static IList<BluetoothFactory> s_factories;
        static readonly object lockKey = new object();

        private static void GetStacks_inLock()
        {
            List<BluetoothFactory> list = new List<BluetoothFactory>();
            IList<Exception> errors = new List<Exception>();
            List<string> stacks = new List<string>(
                BluetoothFactoryConfig.KnownStacks);
            TraceWriteLibraryVersion();
            foreach (string factoryName in stacks)
            {
                try
                {
                    Type t = Type.GetType(factoryName, true);
                    Debug.Assert(t != null, string.Format(System.Globalization.CultureInfo.InvariantCulture,
                            "Expected GetType to throw when type not found: '{0}'", factoryName));
                    object tmp = Activator.CreateInstance(t);
                    Debug.Assert(tmp != null, "Expect all failures to throw rather than return null.");
                    list.Add((BluetoothFactory)tmp);
                    // only one factory
                    break;
                }
                catch (Exception ex)
                {
                    if (ex is System.Reflection.TargetInvocationException)
                    {
                        Debug.Assert(ex.InnerException != null, "We know from the old }catch(TIEX){throw ex.InnerEx;} that this is non-null");
                        ex = ex.InnerException;
                    }
                    errors.Add(ex);
                    string msg = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "Exception creating factory '{0}, ex: {1}", factoryName, ex);
                    Trace.Fail(msg);
                }
            }//for
            if (list.Count == 0)
            {
                foreach (Exception ex in errors)
                {
                    Trace.Fail(ex.Message);
                }
                throw new PlatformNotSupportedException("No supported Bluetooth protocol stack found.");
            }
            else
            {
                SetFactories_inLock(list);
            }
            // result
#if !ANDROID
            Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Num factories: {1}, Primary Factory: {0}",
                (s_factories == null ? "(null)" : s_factories[0].GetType().Name),
                (s_factories == null ? "(null)" : s_factories.Count.ToString())));
#endif
        }

        internal static IList<BluetoothFactory> Factories
        {
            get
            {
                lock (lockKey)
                {
                    if (s_factories == null)
                    {
                        GetStacks_inLock();
                    }//if
                    Debug.Assert(s_factories.Count > 0, "Empty s_factories!");
#if !V1 // Have to suffer mutableness in NETCFv1. :-(
                    Debug.Assert(((System.Collections.IList)s_factories).IsReadOnly, "!IsReadOnly");
                    Debug.Assert(((System.Collections.IList)s_factories).IsFixedSize, "!IsFixedSize");
#endif
                    return s_factories;
                }
            }
        }

        internal static BluetoothFactory Factory
        {
            get { return Factories[0]; /* cast for NETCFv1 */ }
        }

        internal static void SetFactory(BluetoothFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            lock (lockKey)
            {
                Debug.WriteLine("SetFactory: " + factory == null ? "(null)" : factory.GetType().Name);
                SetFactories_inLock(new List<BluetoothFactory>(new BluetoothFactory[] { factory }));
            }
        }

        private static void SetFactories_inLock(List<BluetoothFactory> list)
        {
            Debug.Assert(list.Count > 0, "Empty s_factories!");
#if !V1
            s_factories = list.AsReadOnly();
#else
            s_factories = list; // warning not ReadOnly
#endif
        }

        //--------------------------------------------------------------
        public static void HackShutdownAll()
        {
            lock (lockKey)
            {
                if (s_factories != null)
                    foreach (BluetoothFactory cur in s_factories)
                    {
                        ((IDisposable)cur).Dispose();
                    }
            }//lock
            s_factories = null;
        }


        /// <summary>
        /// PRE-RELEASE
        /// Get the instance of the given factory type -- if it exists.
        /// </summary>
        /// -
        /// <typeparam name="TFactory">The factory type e.g.
        /// <see cref="T:InTheHand.Net.Bluetooth.SocketsBluetoothFactory"/>
        /// or <see cref="T:InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase"/>
        /// etc.
        /// </typeparam>
        /// -
        /// <returns>The instance of the given type or <c>null</c>.
        /// </returns>
        public static TFactory GetTheFactoryOfTypeOrDefault<TFactory>()
            where TFactory : BluetoothFactory
        {
            foreach (BluetoothFactory curF in Factories)
            {
                if (curF is TFactory)
                {
                    return (TFactory)curF;
                }
            }//for
            return null;
        }

        /// <summary>
        /// PRE-RELEASE
        /// Get the instance of the given factory type -- if it exists.
        /// </summary>
        /// -
        /// <param name="factoryType">The factory type e.g.
        /// <see cref="SocketsBluetoothFactory"/>
        /// etc.
        /// </param>
        /// -
        /// <returns>The instance of the given type or <c>null</c>.
        /// </returns>
        public static BluetoothFactory GetTheFactoryOfTypeOrDefault(Type factoryType)
        {
            foreach (BluetoothFactory curF in BluetoothFactory.Factories)
            {
                if (factoryType.IsInstanceOfType(curF))
                {
                    return curF;
                }
            }//for
            return null;
        }

        //----
        private static void TraceWriteLibraryVersion()
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            //var an = new System.Reflection.AssemblyName(fn);
            System.Reflection.AssemblyName an = a.GetName();
            Version v = an.Version; // Is AssemblyVersionAttribute.
            //
            System.Reflection.AssemblyInformationalVersionAttribute aiva = GetCustomAttributes<System.Reflection.AssemblyInformationalVersionAttribute>(a);
            string vi = aiva?.InformationalVersion;
            //
            Trace.WriteLine(string.Format("32feet.NET: '{0}'\r\n   versions: '{1}' and '{2}'.",
                an, v, vi));
        }

        static TAttr GetCustomAttributes<TAttr>(System.Reflection.Assembly a)
            where TAttr : Attribute
        {
            object[] arr = a.GetCustomAttributes(typeof(TAttr), true);
#if false // _Not_ supported on NETCF
            var newArr = Array.ConvertAll(arr, x => (TAttr)x);
#endif
            if (arr.Length == 0)
                return null;
            if (arr.Length > 1)
            {
                throw new InvalidOperationException("Don't support multiple attribute instances.");
            }
            TAttr attr = (TAttr)arr[0];
            return attr;
        }

    }
}
