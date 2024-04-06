using Microsoft.AspNetCore.Authentication.Cookies;
using SkiaSharp;

namespace AddCCFlow.Extensions
{
    /// <summary>
    /// 服务扩展静态类,包含服务扩展方法
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// CORS 配置
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureCors(this IServiceCollection services, string AllowSpecificOrigins)
        {
            //将策略名称设置为 _myAllowSpecificOrigins。 策略名称是任意的。
            var MyAllowSpecificOrigins = AllowSpecificOrigins;

            // 调用 UseCors 扩展方法并指定 _myAllowSpecificOrigins CORS 策略。
            //UseCors 添加 CORS 中间件。 对 UseCors 的调用必须放在 UseRouting 之后，但在 UseAuthorization 之前。
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:8000",
                                    "https://localhost:8000") // 指定前端地址
                        .AllowAnyHeader() // 用于支持http头中带token的认证
                        .AllowAnyMethod() // 跨域时，浏览器会发一个OPTIONS请求。这儿也可以用.WithHeaders来指定具体的Http方法
                        .SetPreflightMaxAge(TimeSpan.FromDays(1)) // 将预检请求的结果缓存, 即设置Access-Control-Max-Age头
                        .AllowCredentials(); // 设置 Access-Control-Allow-Credentials 标头
                    });
            });
        }

        /// <summary>
        /// IIS 配置作为 .NET Core 配置的一部分
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options =>
            {

            });
        }

        /// <summary>
        /// EF Core配置
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureAddDbContext(this IServiceCollection services)
        {
            //services.AddDbContextFactory<OAContext>(options =>
            //{
            //    string DbConnectionString = ConnectionString;
            //    options.UseSqlServer(i => i.UseRowNumberForPaging())
            //    .EnableDetailedErrors();
            //});
        }

        /// <summary>
        /// Authentication 配置
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureAddAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            //.AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters()
            //    {
            //        ValidateIssuer = true, //是否验证Issuer
            //        ValidIssuer = configuration["Jwt:Issuer"], //发行人Issuer
            //        ValidateAudience = true, //是否验证Audience
            //        ValidAudience = configuration["Jwt:Audience"], //订阅人Audience
            //        ValidateIssuerSigningKey = true, //是否验证SecurityKey
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])), //SecurityKey
            //        ValidateLifetime = true, //是否验证失效时间
            //        ClockSkew = TimeSpan.FromSeconds(30), //过期时间容错值，解决服务器端时间不同步问题（秒）
            //        RequireExpirationTime = true,
            //    };
            //})
            ;
        }


    }
}
