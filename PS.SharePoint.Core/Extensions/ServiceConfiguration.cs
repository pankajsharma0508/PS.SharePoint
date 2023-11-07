using PS.SharePoint.Core.Entities;
using PS.SharePoint.Core.Interfaces;
using PS.SharePoint.Core.Repository;
using Unity;

namespace PS.SharePoint.Core.Extensions
{
    public static class ServiceConfiguration
    {
        private static IUnityContainer unityContainer;

        public static void RegisterSpConfiguration(this IUnityContainer container, SharePointConfiguration configuration)
        {
            container.RegisterInstance<IContextManager>(new SharePointContextManager(configuration));
            unityContainer = container;
        }

        public static void RegisterSpRepository<T>(this IUnityContainer container)
        {
            container.RegisterType(typeof(ISharePointRepository<>), typeof(BaseRepository<>));
            unityContainer = container;
        }

        public static IUnityContainer GetContainer()
        {
            return unityContainer;
        }
    }
}



