using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using SwiftCode.BBS.Common.Helper;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton(new AppSettingsHelper(builder.Configuration));

// Swagger Configuration
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v0.0.1",
        Title = "SwiftCode.BBS.API",
        Description = "Description for Frame",
        Contact = new OpenApiContact { Name = "AlbertZhao", Email = "szdxzhy@outlook.com" }
    });
    var basePath = AppContext.BaseDirectory;
    // SwiftCode.BBS.API.xml
    var xmlPath = Path.Combine(basePath, "SwiftCode.BBS.API.xml");
    options.IncludeXmlComments(xmlPath, true);
    // SwiftCode.BBS.Model.xml
    xmlPath = Path.Combine(basePath, "SwiftCode.BBS.Model.xml");
    options.IncludeXmlComments(xmlPath);

    // 开启小锁  Swagger启用授权输入
    options.OperationFilter<AddResponseHeadersFilter>();
    options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

    // 在header中添加token，传递到后台
    options.OperationFilter<SecurityRequirementsOperationFilter>();
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
        Name = "Authorization",//jwt默认的参数名称
        In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
        Type = SecuritySchemeType.ApiKey,
    });
});

// Authorize Policy 授权 System.IdentityModel.Tokens.Jwt
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build()); // Admin角色
    options.AddPolicy("System", policy => policy.RequireRole("System").Build());// System角色
    options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System"));//或的关系
    options.AddPolicy("SystemAndAdmin", policy => policy.RequireRole("Admin").RequireRole("System"));//且的关系
});

// AuAuthentication认证 Microsoft.AspNetCore.Authentication.JwtBearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    //读取配置文件
    var audienceConfig = builder.Configuration.GetSection("Audience");
    var symmetricKeyAsBase64 = audienceConfig["Secret"];
    var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
    var signingKey = new SymmetricSecurityKey(keyByteArray);

    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateIssuer = true,
        ValidIssuer = audienceConfig["Issuer"],//发行人
        ValidateAudience = true,
        ValidAudience = audienceConfig["Audience"],//订阅人
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,//这个是缓冲过期时间，也就是说，即使我们配置了过期时间，这里也要考虑进去，过期时间+缓冲，默认好像是7分钟，你可以直接设置为0
        RequireExpirationTime = true,
    };
});

var app = builder.Build();

// Use Swagger
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiHelp v0.0.1");
    //c.RoutePrefix = " ";// set it for production.yong'
});

// 一定要注意中间件的顺序！！！ 先开启认证，然后是授权检测。
// 先开启认证
app.UseAuthentication();

// 然后是授权
app.UseAuthorization();



app.MapControllers();

app.Run();
