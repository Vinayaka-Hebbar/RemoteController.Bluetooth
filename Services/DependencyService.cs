using System;
using System.Threading;

namespace RemoteController
{
    public class DependencyService : IServiceProvider
    {
        static volatile DependencyService _instance;

        public static DependencyService Instance
        {
            get
            {
                if (_instance == null)
                    Interlocked.Exchange(ref _instance, new DependencyService());
                return _instance;
            }
        }

        readonly System.ComponentModel.Design.ServiceContainer _container;

        private DependencyService()
        {
            _container = new System.ComponentModel.Design.ServiceContainer();
            _container.AddService(typeof(IServiceProvider), this);
        }

        public void Register<TService>() where TService : new()
        {
            _container.AddService(typeof(TService), (c, t) =>
            {
                return new TService();
            });
        }

        public TService GetService<TService>()
        {
            return (TService)_container.GetService(typeof(TService));
        }

        public object GetService(Type serviceType)
        {
            return _container.GetService(serviceType);
        }
    }
}
