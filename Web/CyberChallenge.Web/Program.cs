namespace CyberChallenge.Web
{
    using System.Reflection;

    using CyberChallenge.Data;
    using CyberChallenge.Data.Common;
    using CyberChallenge.Data.Common.Repositories;
    using CyberChallenge.Data.Models;
    using CyberChallenge.Data.Repositories;
    using CyberChallenge.Data.Seeding;
    using CyberChallenge.Services.Data;
    using CyberChallenge.Services.Data.Game;
    using CyberChallenge.Services.Data.News;
    using CyberChallenge.Services.Data.Statistics;
    using CyberChallenge.Services.Data.Users;
    using CyberChallenge.Services.Mapping;
    using CyberChallenge.Services.Messaging;
    using CyberChallenge.Web.ViewModels;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);
            var app = builder.Build();
            Configure(app);
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.CommandTimeout(180))
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
            services.Configure<CookiePolicyOptions>(
                options =>
                {
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });

            services.AddControllersWithViews(
                options =>
                {
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                }).AddRazorRuntimeCompilation();
            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddSingleton(configuration);

            // WebOptimizer (bundling and minification)
            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddCssBundle("/css/site.min.css", "css/site.css");
                pipeline.AddJavaScriptBundle("/js/site.min.js", "js/site.js");

                // The only jQuery 4 compatible jquery-validation-unobtrusive build is an unreleased
                // upstream commit that ships no .min.js file (see libman.json), so minify it here.
                pipeline.MinifyJsFiles("lib/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js");
            });

            // Data repositories
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

            // Application services
            services.AddTransient<IEmailSender, NullMessageSender>();
            services.AddTransient<ISettingsService, SettingsService>();

            // Сервис административного управления пользователями.
            services.AddScoped<IUserManagementService, UserManagementService>();

            // Сервис управления новостной лентой.
            services.AddScoped<INewsService, NewsService>();

            // Сервис игровой логики: уровни и проверка решений.
            services.AddScoped<IGameService, GameService>();

            // Сервис расчета очков уровня.
            services.AddScoped<GameScoreCalculator>();

            // Сервис агрегированной статистики и таблицы лидеров.
            services.AddScoped<IStatisticsService, StatisticsService>();
        }

        private static void Configure(WebApplication app)
        {
            // Seed data on application startup
            using (var serviceScope = app.Services.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                new ApplicationDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
            }

            MappingConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseWebOptimizer();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
        }
    }
}
