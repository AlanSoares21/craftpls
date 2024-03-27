using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using AlternativeMkt;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using AlternativeMkt.Middlewares;
using AlternativeMkt.Services;
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
builder.Services.AddScoped<ICraftResourceService, CraftResourceService>();
builder.Services.AddScoped<IPriceService, PriceService>();
builder.Services.AddScoped<ICraftItemService, CraftItemService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<ServerConfig>();
builder.Services.AddScoped<IDateTools, DateTools>();
builder.Services.AddScoped<AuthMiddleware>();

var config = new ServerConfig(builder.Configuration);
string adminRoleId = config.AdminRoleId.ToString();

builder.Services.AddCors(options => {
    options.AddPolicy(name: CorsPolicyName, policy => {
        Console.WriteLine($"Allowed origin: {config.AllowedOrigin}");
        policy.WithOrigins(config.AllowedOrigin)
            .WithMethods("POST", "PUT", "DELETE", "GET", "OPTIONS")
            .AllowAnyHeader();
    });
});


builder.Services.AddAuthorization(options => {
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimTypes.Sid);
    });
    options.AddPolicy("AdminAccess", policy => {
        policy.AddAuthenticationSchemes("AdminAccess");
        policy.RequireRole("admin");
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
            NameClaimType = ClaimTypes.NameIdentifier,
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

app.UseCors(CorsPolicyName);
app.UseMiddleware<AuthMiddleware>();

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
app.UseAuthentication();

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new {
        area = "main",
        controller = "Home",
        action = "Index"
    }
);

app.MapAreaControllerRoute(
    name: "default",
    areaName: "main|admin",
    pattern: "{area=main}/{controller=Home}/{action=Index}/{id?}",
    defaults: new {
        area = "main",
        controller = "Home",
        action = "Index"
    },
    constraints: new { area = "main|admin" }
);

app.Run();