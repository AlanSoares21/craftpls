using System.Security.Claims;
using System.Text.Json.Serialization;
using AlternativeMkt;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

const string CorsPolicyName = "CorsDefault";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocalization(opt => {
    opt.ResourcesPath = "Resources";
});

builder.Services.AddControllers()
    .AddJsonOptions(opt => {
        opt.JsonSerializerOptions.ReferenceHandler = 
            ReferenceHandler.IgnoreCycles;
    });

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization();

builder.Services.AddDbContext<MktDbContext>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ServerConfig>();
builder.Services.AddScoped<IDateTools, DateTools>();

var config = new ServerConfig(builder.Configuration);
string adminRoleId = config.AdminRoleId.ToString();

builder.Services.AddCors(options => {
    options.AddPolicy(name: CorsPolicyName, policy => {
        Console.WriteLine($"Allowed origin: {config.AllowedOrigin}");
        policy.WithOrigins(config.AllowedOrigin)
            .WithMethods("POST", "PUT", "GET", "OPTIONS")
            .AllowAnyHeader();
    });
});


builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminAccess", policy => {
        policy.RequireRole(adminRoleId);
    });
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimTypes.NameIdentifier);
    });
});


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            IssuerSigningKey = new SymmetricSecurityKey(config.SecretKey),
            LifetimeValidator = (before, expires, token, parameters) => expires > DateTime.UtcNow,
            ValidIssuer = config.Issuer,
            ValidAudience = config.Audience,
            NameClaimType = ClaimTypes.NameIdentifier
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Cookies["Identifier"];
                
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = context.Request.Cookies["Identifier"];
                } else {
                    accessToken = context.Request.Cookies["Authorization"];
                    if (!string.IsNullOrEmpty(accessToken))
                        context.Token = accessToken["Bearer ".Length..];
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseCors(CorsPolicyName);

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new {
        area = "main",
        culture = "en",
        controller = "ChangeCulture",
        action = "RedirectToLocalized"
    }
);

app.MapAreaControllerRoute(
    name: "default",
    areaName: "main|admin",
    pattern: "{area=main}/{culture}/{controller=Home}/{action=Index}/{id?}",
    defaults: new {
        area = "main",
        culture = "en",
        controller = "Home",
        action = "Index"
    },
    constraints: new { area = "main|admin", culture = "en|pt" }
);

app.Run();