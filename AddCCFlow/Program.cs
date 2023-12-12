using BP.Difference;
using BP.Web;
using CCFlow;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);
var ApiName = "CCFlow.Core";
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
            policy =>
            {
                policy.WithOrigins("http://localhost:8000", "https://localhost:8000") // 指定前端地址
                .AllowAnyHeader() // 用于支持http头中带token的认证
                .AllowAnyMethod() // 跨域时，浏览器会发一个OPTIONS请求。这儿也可以用.WithHeaders来指定具体的Http方法
                .SetPreflightMaxAge(TimeSpan.FromDays(1)) // 将预检请求的结果缓存, 即设置Access-Control-Max-Age头
                .AllowCredentials(); // 设置 Access-Control-Allow-Credentials 标头
            });
});

builder.Services.AddControllers().AddControllersAsServices(); ;
builder.Services.AddDistributedMemoryCache();
builder.Services.AddRazorPages();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// 解决异常：System.InvalidOperationException: Synchronous operations are disallowed. Call WriteAsync or set AllowSynchronousIO to true instead.
// 原因参见： https://github.com/aspnet/AspNetCore/issues/8302
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
var basePath = AppDomain.CurrentDomain.BaseDirectory;

builder.Services.AddSwaggerGen();
// 用于类库中访问HttpContext，等价于： services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // 因为asp.net core 2.1 开始遵循欧盟GDPR规则，所以需要启用下面的两个配置
    // 参考文章： https://andrewlock.net/session-state-gdpr-and-non-essential-cookies/
    // 是否需要用户手动确认接受cookie，配置为false，表示不需要用户确认
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    // options.SameSite = SameSiteMode.None; 
});

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AdventureWorks.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(2000);//设置session的过期时间
    options.Cookie.HttpOnly = true;//设置在浏览器不能通过js获得该cookie的值 
    options.Cookie.IsEssential = true;
});

//builder.Services.AddTransient<IStartupFilter, RequestSetOptionsStartupFilter>();

var app = builder.Build();

// Configure the HTTP request pipeline.配置HTTP请求管道。
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// 使用静态文件访问
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
});
app.UseSession();

app.UseRouting();

// 跨域
app.UseCors(MyAllowSpecificOrigins);
// 启用全局Cookie配置
//app.UseCookiePolicy();
app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
});
//ccflow核心中间件
app.UseCcHandler();
//app.UseRequestCulture();

app.UseAuthorization();

app.MapControllers();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//    endpoints.MapControllerRoute(
//        name: "default",
//        pattern: "{controller=Home}/{action=Index}/{id?}");

//    endpoints.MapControllerRoute(
//        name: "areas",
//        pattern: "{area}/{controller}/{action=Index}/{id?}");
//});
app.UseStaticHttpContext();

// En30里面用到
HttpContextHelper.PhysicalApplicationPath = builder.Environment.ContentRootPath + Path.DirectorySeparatorChar; ;
NetCoreAppHelper.ServiceProvider = ((IApplicationBuilder)app).ApplicationServices;

app.Run();
