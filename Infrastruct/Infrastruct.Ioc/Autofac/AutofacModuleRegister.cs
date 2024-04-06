using Autofac;
using Infrastruct.Ioc.Interceptors;

namespace Infrastruct.Ioc.Autofac
{
    /// <summary>
    /// 定义Module，方便对注入服务进行管理
    /// </summary>
    public class AutofacModuleRegister : Module
    {
        /// <summary>
        /// 重写Autofac管道oad方法，在这里注册注入
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            //注册拦截器到Autofac容器
            builder.RegisterType<DbTranInterceptor>();

            //builder.RegisterType<ArticleService>().As<IArticleService>();

            #region EF
            // 首先注册 options，供 DbContext 服务初始化使用
            //builder.Register(c =>
            //{
            //    var optionsBuilder = new DbContextOptionsBuilder<OAContext>();
            //    optionsBuilder.UseSqlServer(b => b.UseRowNumberForPaging())
            //    .EnableDetailedErrors();
            //        //.MigrationsAssembly("BookList.Domain"));
            //    return optionsBuilder.Options;
            //}).InstancePerLifetimeScope();
            //// 注册 DbContext
            //builder.RegisterType<OAContext>()
            //    .AsSelf()
            //    .InstancePerLifetimeScope();
            #endregion

            /* 程序集注入业务服务
             * 新建接口层和服务层类库，这里我用了同一个命名空间"Application"
             * （以前的写法，建议改改，改成Interface和Service这两个名称，分别对应那两行代码），
             * 建完这两个类库以后，在WebAPI层需要引用一下这两个类库，否则就会提示你截图那个问题，
             * 本质上是因为在反射当前程序集时，未能找到引用。
             */
            //var IAppServices = System.Reflection.Assembly.Load("Interface");
            //var AppServices = System.Reflection.Assembly.Load("Service");
            //// 根据名称约定（服务层的接口和实现均以Service结尾），实现服务接口和服务实现的依赖
            //builder.RegisterAssemblyTypes(IAppServices, AppServices)
            //    .Where(t => t.Name.EndsWith("Service"))
            //    .AsImplementedInterfaces();

            /*
             * 找出当前模块文件所在程序集中的所有类型注册为其实现的服务接口，注册模式为生命周期模式。
             * 这里跟旧版本的MVC或API有点儿不同的地方，旧版本用的是InstancePerRquest，但Core下面已经没有这种模式了，
             * 而是InstancePerLifetimeScope，起同样的效果。这里，我所有的服务类都以Service结尾。
             */
            //builder.RegisterAssemblyTypes(ThisAssembly)
            //    .Where(t => t.Name.EndsWith("Service"))
            //    .AsImplementedInterfaces()
            //    .InstancePerLifetimeScope();
        }
    }
}
