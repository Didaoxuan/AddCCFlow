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
}
