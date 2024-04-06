using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infrastruct.Ioc.Autofac;

namespace AddCCFlow.Extensions
{
    ///<summary>
    ///HostBuilder拓展
    ///</summary>
    public static class AutofacHostBuilderExtensions
    {
        /// <summary>
        /// 创建服务提供工厂,注入ContainerBuilder
        /// </summary>
        /// <param name="hostBuiler"></param>
        /// <returns></returns>
        public static IHostBuilder UseAutofac1(this IHostBuilder hostBuilder)
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
