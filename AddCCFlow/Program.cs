using AddCCFlow.Extensions;
using BP.Difference;
using BP.Web;
using CCFlow;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Infrastruct.Ioc.Autofac;

var configurationRoot = new ConfigurationBuilder()
#if DEBUG
 .AddJsonFile("appsettings.Development.json")
#else
 .AddJsonFile("appsettings.json")
#endif
.Build();
Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configurationRoot)
                .CreateLogger();
try
{
    Log.Information("Starting Acme.BookStoreABP.HttpApi.Host.");

    var builder = WebApplication.CreateBuilder(args);
    var ApiName = "CCFlow.Core";
    var AllowSpecificOrigins = "_myAllowSpecificOrigins";
    var basePath = AppDomain.CurrentDomain.BaseDirectory;

    builder.Services.ConfigureCors(AllowSpecificOrigins);
    
    builder.Services.AddControllers().AddControllersAsServices(); ;
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddRazorPages();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    // ����쳣��System.InvalidOperationException: Synchronous operations are disallowed. Call WriteAsync or set AllowSynchronousIO to true instead.
    // ԭ��μ��� https://github.com/aspnet/AspNetCore/issues/8302
    builder.Services.Configure<IISServerOptions>(options =>
    {
        options.AllowSynchronousIO = true;
    });
    builder.Services.Configure<KestrelServerOptions>(options =>
    {
        options.AllowSynchronousIO = true;
    });

    builder.Services.AddSwaggerGen();
    // ��������з���HttpContext���ȼ��ڣ� services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddHttpContextAccessor();

    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        // ��Ϊasp.net core 2.1 ��ʼ��ѭŷ��GDPR����������Ҫ�����������������
        // �ο����£� https://andrewlock.net/session-state-gdpr-and-non-essential-cookies/
        // �Ƿ���Ҫ�û��ֶ�ȷ�Ͻ���cookie������Ϊfalse����ʾ����Ҫ�û�ȷ��
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
        options.IdleTimeout = TimeSpan.FromSeconds(2000);//����session�Ĺ���ʱ��
        options.Cookie.HttpOnly = true;//���������������ͨ��js��ø�cookie��ֵ
        options.Cookie.IsEssential = true;
    });

    builder.Services.AddAutoMapperSetup();
    builder.Host.AddAppSettingsSecretsJson()
        .UseAutofac<ModuleRegister>()
        .UseSerilog();

    var app = builder.Build();

    // Configure the HTTP request pipeline.����HTTP����ܵ���
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    // ʹ�þ�̬�ļ�����
    app.UseStaticFiles(new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
    });
    app.UseSession();

    app.UseRouting();

    // ����
    app.UseCors(AllowSpecificOrigins);
    // ����ȫ��Cookie����
    app.UseCookiePolicy(new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.Always,
        HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
    });
    //ccflow�����м��
    app.UseCcHandler();
    //app.UseRequestCulture();

    app.UseAuthorization();

    app.MapControllers();

    app.UseStaticHttpContext();

    // En30�����õ�
    HttpContextHelper.PhysicalApplicationPath = builder.Environment.ContentRootPath + Path.DirectorySeparatorChar; ;
    NetCoreAppHelper.ServiceProvider = ((IApplicationBuilder)app).ApplicationServices;

    app.Run();
    return 0;
}
catch (Exception ex)
{
    if (ex is HostAbortedException)
    {
        throw;
    }

    Log.Fatal(ex, "Host terminated unexpectedly!");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}