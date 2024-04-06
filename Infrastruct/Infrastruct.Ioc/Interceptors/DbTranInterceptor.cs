using Castle.DynamicProxy;
using Domain.Interfaces.UnitOfWork;
using Infrastruct.Ioc.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Infrastruct.Ioc.Interceptors
{
    /// <summary>
    /// 数据库事务拦截器 继承Castle.DynamicProxy.IInterceptor接口
    /// </summary>
    public class DbTranInterceptor(IUnitOfWork unitOfWorkManage, ILogger<DbTranInterceptor> logger, DbContext context) : IInterceptor
    {

        /// <summary>
        /// 实例化IInterceptor唯一方法 
        /// </summary>
        /// <param name="invocation">包含被拦截方法的信息</param>
        public void Intercept(IInvocation invocation)
        {
            //获取被拦截的方法对象
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            //检查当前方法是否有[UseTran]特性修饰 
            if (method.GetCustomAttribute<UseTranAttribute>(true) is { } uta)
            {
                //开启事务
                using var transaction = context.Database.BeginTransaction();
                try
                {

                    //_unitOfWorkManage.BeginTran(); 

                    invocation.Proceed();

                    context.SaveChanges();

                    //判断方法是否为异步方法，异步方法则需要Wait等待
                    if (IsAsyncMethod(invocation.Method))
                    {
                        var result = invocation.ReturnValue;
                        if (result is Task)
                        {
                            //Task.WaitAll(result as Task);
                            (result as Task).Wait();
                        }
                    }

                    //提交事务
                    transaction.Commit();
                    //_unitOfWorkManage.CommitTran();   
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());

                    //回滚事务
                    //用Using包裹的话，不再需要手动Rollback，走完Using会自动回滚。
                    //如果不用Using包裹事务，就需要在Catch中手动RollBack回滚，并且最好最后手动的Dispose一下。
                    transaction.Rollback();
                    //_unitOfWorkManage.RollbackTran(); 
                    throw;
                }
            }
            else
            {
                invocation.Proceed(); //如果方法没有[UseTran]则不需要开启事务，直接执行被拦截方法
            }
        }

        /// <summary>
        /// 判断方法是否为异步方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsAsyncMethod(MethodInfo method)
        {
            return (
                method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            );
        }
    }
}
