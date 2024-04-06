using AspectCore.Extensions.Autofac;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastruct.Ioc.Autofac
{
    /// <summary>
    /// Autofac Ioc容器替换.NET 的Ioc
    /// </summary>
    public static class AutofacExtensions
    {
        public static IServiceProvider UseAutofac<TModule>(this IServiceCollection services, Action<ConfigInjection> config = null)
           where TModule : Module, new()
        {
            ContainerBuilder builder = new();
            builder.Populate(services);

            ConfigInjection defaultConfig = ConfigInjection.Default(builder);
            //封装具有单个参数并且不返回值的方法。
            config?.Invoke(defaultConfig);

            builder.RegisterModule<TModule>();
            builder.RegisterDynamicProxy();
            IContainer container = builder.Build();
            return new AutofacServiceProvider(container);
        }

        /// <summary>
        /// HostBuilder拓展
        /// 创建服务提供工厂,注入ContainerBuilder
        /// </summary>
        /// <param name="hostBuiler"></param>
        /// <returns></returns>
        public static IHostBuilder UseAutofac<TModule>(this IHostBuilder hostBuilder)
            where TModule : Module, new()
        {
            //hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            return hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule<TModule>();
                    //这两种注册方式达到的效果都一样
                    //builder.RegisterModule(new TModule()); //Autofac模块集成及AutoMapper配置
                });
        }

        /// <summary>
        /// HostBuilder拓展
        /// 创建服务提供工厂,注入ContainerBuilder
        /// </summary>
        /// <param name="hostBuiler"></param>
        /// <returns></returns>
        public static IHostBuilder UseAutofac(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterModule<AutofacModuleRegister>();
                    //这两种注册方式达到的效果都一样
                    //builder.RegisterModule(new AutofacModuleRegister()); //Autofac模块集成及AutoMapper配置
                });
        }
    }
}
