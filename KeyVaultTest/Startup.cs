using KeyVaultTest.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using System;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KeyVaultTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public SnapshotCollectorTelemetryProcessorFactory(IServiceProvider serviceProvider) =>
                _serviceProvider = serviceProvider;

            public ITelemetryProcessor Create(ITelemetryProcessor next)
            {
                var snapshotConfigurationOptions = _serviceProvider.GetService<IOptions<SnapshotCollectorConfiguration>>();
                return new SnapshotCollectorTelemetryProcessor(next, configuration: snapshotConfigurationOptions.Value);
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //services.AddCors(options =>
            //{
            //    options.AddPolicy(_allowedOriginsPolicy,
            //    builder =>
            //    {
            //        builder.WithOrigins("https://login.microsoftonline.com",
            //                             "https://localhost:44319/")
            //                            .AllowAnyHeader()
            //                            .AllowAnyMethod();
            //    });
            //});

            /* Authentication to AAD Directory */
            services.AddAuthentication(opt =>
            {

                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = Configuration["AzureAd:Instance"] + Configuration["AzureAd:TenantId"];

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidAudiences = new List<string>
                    {
                        Configuration["AzureAd:ClientId"], // For ReactJS Client App
                        $"api://{Configuration["AzureAd:ClientId"]}" // Xamarin Mobile Client App 
                    }
                };

                if (Configuration.GetChildren().Any(item => item.Key == "RequireHttpsMetadata"))
                    options.RequireHttpsMetadata = Configuration.GetValue<bool>("RequireHttpsMetadata");
            });

            services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                    .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));


            //services.AddMicrosoftIdentityPlatformAuthentication(Configuration);


            /* AddAuthorization sections helps us get groups from AAD */
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Configuration["AzureSecurityGroup:CoalitionReadOnlyObjectLabel"],
                    policyBuilder => policyBuilder.RequireClaim("groups",
                    Configuration["AzureSecurityGroup:CoalitionReadOnlyObjectId"]));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Configuration["AzureSecurityGroup:CoalitionEditObjectLabel"],
                    policyBuilder => policyBuilder.RequireClaim("groups",
                    Configuration["AzureSecurityGroup:CoalitionEditObjectId"]));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Configuration["AzureSecurityGroup:CoalitionAdminObjectLabel"],
                    policyBuilder => policyBuilder.RequireClaim("groups",
                    Configuration["AzureSecurityGroup:CoalitionAdminObjectId"]));
            });

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("coalition-read",
            //        policyBuilder => policyBuilder.RequireClaim("groups",
            //        "bead8c38-cbd4-4c5c-913a-3db9d776ebad"));
            //});
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("coalition-edit",
            //        policyBuilder => policyBuilder.RequireClaim("groups",
            //        "a2454ce9-11c5-4af2-9f2a-5730591356ec"));
            //});
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("coalition-admin",
            //        policyBuilder => policyBuilder.RequireClaim("groups",
            //        "60f2493c-9efa-4985-b8c9-e8832a46ef56"));
            //});

            // Used for pulling user info through DI HTTPContext
            services.AddHttpContextAccessor();

            // DI for configuration
            services.Configure<SecurityGroups>(Configuration.GetSection("AzureSecurityGroup"));
            // Use app insights for logging
            services.AddApplicationInsightsTelemetry(Configuration);

            // Configure SnapshotCollector from application settings
            services.Configure<SnapshotCollectorConfiguration>(Configuration.GetSection(nameof(SnapshotCollectorConfiguration)));

            // Add SnapshotCollector telemetry processor.
            services.AddSingleton<ITelemetryProcessorFactory>(sp => new SnapshotCollectorTelemetryProcessorFactory(sp));


            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
