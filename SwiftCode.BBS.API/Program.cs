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

    // ����С��  Swagger������Ȩ����
    options.OperationFilter<AddResponseHeadersFilter>();
    options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

    // ��header�����token�����ݵ���̨
    options.OperationFilter<SecurityRequirementsOperationFilter>();
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "JWT��Ȩ(���ݽ�������ͷ�н��д���) ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�\"",
        Name = "Authorization",//jwtĬ�ϵĲ�������
        In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
        Type = SecuritySchemeType.ApiKey,
    });
});

// Authorize Policy ��Ȩ System.IdentityModel.Tokens.Jwt
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build()); // Admin��ɫ
    options.AddPolicy("System", policy => policy.RequireRole("System").Build());// System��ɫ
    options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System"));//��Ĺ�ϵ
    options.AddPolicy("SystemAndAdmin", policy => policy.RequireRole("Admin").RequireRole("System"));//�ҵĹ�ϵ
});

// AuAuthentication��֤ Microsoft.AspNetCore.Authentication.JwtBearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    //��ȡ�����ļ�
    var audienceConfig = builder.Configuration.GetSection("Audience");
    var symmetricKeyAsBase64 = audienceConfig["Secret"];
    var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
    var signingKey = new SymmetricSecurityKey(keyByteArray);

    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateIssuer = true,
        ValidIssuer = audienceConfig["Issuer"],//������
        ValidateAudience = true,
        ValidAudience = audienceConfig["Audience"],//������
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,//����ǻ������ʱ�䣬Ҳ����˵����ʹ���������˹���ʱ�䣬����ҲҪ���ǽ�ȥ������ʱ��+���壬Ĭ�Ϻ�����7���ӣ������ֱ������Ϊ0
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

// һ��Ҫע���м����˳�򣡣��� �ȿ�����֤��Ȼ������Ȩ��⡣
// �ȿ�����֤
app.UseAuthentication();

// Ȼ������Ȩ
app.UseAuthorization();



app.MapControllers();

app.Run();
