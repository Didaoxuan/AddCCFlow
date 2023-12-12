namespace CCFlow
{
    //创建扩展方法以通过 IApplicationBuilder 公开中间件：
    public static class CcHandlerAppBuilderExtensions
    {
        //.NET 5 使用
        public static IApplicationBuilder UseCcHandler(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            return app.UseMiddleware<CcHandlerMiddleware>();
        }

        //.NET 8 使用
        public static IApplicationBuilder UseRequestCulture(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CcHandlerMiddleware>();
        }
    }

    public class RequestSetOptionsStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            ArgumentNullException.ThrowIfNull(next);
            return builder =>
            {
                builder.UseMiddleware<CcHandlerMiddleware>();
                next(builder);
            };
        }
    }
}
