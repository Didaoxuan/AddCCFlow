namespace CCFlow
{
    public static class CcHandlerAppBuilderExtensions
    {
        public static IApplicationBuilder UseCcHandler(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            return app.UseMiddleware<CcHandlerMiddleware>();
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
