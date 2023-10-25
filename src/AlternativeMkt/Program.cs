using System.Security.Claims;
using AlternativeMkt;
using AlternativeMkt.Auth;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocalization(opt => {
    opt.ResourcesPath = "Resources";
});

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization();

builder.Services.AddDbContext<MktDbContext>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ServerConfig>();

var config = new ServerConfig(builder.Configuration);

builder.Services.AddAuthorization(options => {
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

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new {
        culture = "en",
        controller = "ChangeCulture",
        action = "RedirectToLocalized"
    }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{culture}/{controller=Home}/{action=Index}/{id?}",
    defaults: new {
        culture = "en",
        controller = "Home",
        action = "Index"
    },
    constraints: new { culture = "en|pt" }
);

app.Run();
