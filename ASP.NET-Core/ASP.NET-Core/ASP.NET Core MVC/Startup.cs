using ASP.NET_Core_MVC.Models;
using ASP.NET_Core_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ASP.NET_Core_MVC
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        private IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection"))
            );
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireUppercase = false;

                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmation");

            services.Configure<DataProtectionTokenProviderOptions>(options =>
                options.TokenLifespan = TimeSpan.FromHours(5));
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(options =>
                options.TokenLifespan = TimeSpan.FromDays(3));

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                options.EnableEndpointRouting = false;
            }).AddXmlSerializerFormatters();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireClaim("Delete Role", "true"));

                options.AddPolicy("EditRolePolicy",
                    policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirements()));

                options.AddPolicy("AdminRolePolicy",
                    policy => policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") || context.User.IsInRole("Super Admin")
                    ));
            });

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "340089542581-6o7c8rs4p53tpggeg5q58fe1nr3k7i7c.apps.googleusercontent.com";
                    options.ClientSecret = "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";

                    //options.ClientId = _config["AuthenticationAPI:Google:ClientId"];
                    //options.ClientSecret = _config["AuthenticationAPI:Google:ClientSecret"];
                })
                .AddFacebook(options =>
                {
                    options.AppId = "725538866161549";
                    options.AppSecret = "6748c85271c84d5aee4884364d03b66a";

                    //options.AppId = _config["AuthenticationAPI:Facebook:AppId"];
                    //options.ClientSecret = _config["AuthenticationAPI:Facebook:ClientSecret"];

                });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });


            //services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();
            services.AddScoped<IEmployeeRepository, SqlEmployeeRepository>();
            services.AddTransient<IEmailService,EmailService>();

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            services.AddSingleton<DataProtectionPurposeStrings>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }//If the application is in the development environment (IsDevelopment() returns true), the UseDeveloperExceptionPage middleware is added to the pipeline.

            //Allows direct access to static content like CSS or image files without going through MVC routing.
            app.UseStaticFiles();
            //Enforces access control based on defined policies or authentication schemes.
            app.UseAuthentication();
            //app.UseMvcWithDefaultRoute();
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                //routes.MapRoute("default", "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}
