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
                policy.WithOrigins("http://localhost:8000", "https://localhost:8000") // ָ��ǰ�˵�ַ
                .AllowAnyHeader() // ����֧��httpͷ�д�token����֤
                .AllowAnyMethod() // ����ʱ��������ᷢһ��OPTIONS�������Ҳ������.WithHeaders��ָ�������Http����
                .SetPreflightMaxAge(TimeSpan.FromDays(1)) // ��Ԥ������Ľ������, ������Access-Control-Max-Ageͷ
                .AllowCredentials(); // ���� Access-Control-Allow-Credentials ��ͷ
            });
});

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
var basePath = AppDomain.CurrentDomain.BaseDirectory;

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

//builder.Services.AddTransient<IStartupFilter, RequestSetOptionsStartupFilter>();

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
app.UseCors(MyAllowSpecificOrigins);
// ����ȫ��Cookie����
//app.UseCookiePolicy();
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

// En30�����õ�
HttpContextHelper.PhysicalApplicationPath = builder.Environment.ContentRootPath + Path.DirectorySeparatorChar; ;
NetCoreAppHelper.ServiceProvider = ((IApplicationBuilder)app).ApplicationServices;

app.Run();
