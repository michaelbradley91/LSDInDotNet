using System;
using System.Reflection;
using Autofac;

namespace LSDInDotNet.Services
{
    public static class DependencyResolver
    {
        private static readonly Lazy<IContainer> ContainerLazy = new Lazy<IContainer>(CreateContainer);

        public static T Resolve<T>()
        {
            return ContainerLazy.Value.Resolve<T>();
        }

        private static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(DependencyResolver))).AsImplementedInterfaces().AsSelf();
            containerBuilder.RegisterType<LineSegmentDetectorWrapper>().As<ILineSegmentDetector>()
                .WithParameter((pi, c) => pi.ParameterType == typeof(ILineSegmentDetector), (pi, c) => c.Resolve<LineSegmentDetector>());
            return containerBuilder.Build();
        }
    }
}
