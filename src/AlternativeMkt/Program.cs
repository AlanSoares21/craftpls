using AlternativeMkt.Db;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocalization(opt => {
    opt.ResourcesPath = "Resources";
});

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization();
builder.Services.AddDbContext<MktDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
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
