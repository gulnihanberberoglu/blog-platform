using System.Text;
using Blog.Api.Data;
using Blog.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// API controller'ları aktif eder
builder.Services.AddControllers();

// SQLite veritabanı ayarı
// DB dosyası proje kökü altındaki App_Data klasörüne oluşturulur
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    Directory.CreateDirectory(dataDir);

    var dbPath = Path.Combine(dataDir, "blog.db");
    opt.UseSqlite($"Data Source={dbPath}");
});

// Uygulama servisleri (Dependency Injection)
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<JwtService>();

// JWT doğrulama ayarları
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var keyBytes = Encoding.UTF8.GetBytes(jwt.Key);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization();

// Swagger yapılandırması (JWT destekli)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blog Platform API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Bearer {token} şeklinde gir.",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new List<string>() }
    });
});

// Frontend erişimi için CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("fe", p =>
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
         .SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

// CORS middleware
app.UseCors("fe");

// Geliştirme ortamında Swagger açık
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Kimlik doğrulama ve yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

// Controller endpointlerini bağla
app.MapControllers();

// Veritabanı oluşturma ve demo veri ekleme
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var pw = scope.ServiceProvider.GetRequiredService<PasswordService>();

        var demo = new Blog.Api.Models.User
        {
            Email = "demo@ghost.local",
            DisplayName = "Demo",
            PasswordHash = pw.Hash("Demo123!")
        };

        db.Users.Add(demo);
        db.SaveChanges();

        db.Posts.Add(new Blog.Api.Models.Post
        {
            Title = "Blog Platform'a hoş geldin",
            Content =
                "Bu, seed edilen örnek bir post.\n" +
                "Giriş yapıp yeni post oluşturabilir, arama yapabilir, silebilir ve temizleyebilirsin.\n\n" +
                "Demo: demo@ghost.local / Demo123!",
            AuthorId = demo.Id
        });

        db.SaveChanges();
    }
}

app.Run();
