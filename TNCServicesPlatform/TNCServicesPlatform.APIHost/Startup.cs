using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using System.Linq;
using TNCServicesPlatform.DataModel;
using TNCServicesPlatform.DataModel.Interfaces;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using System;

namespace TNCServicesPlatform.APIHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var assemblyFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var builder = services.AddMvc();
            // Register API Library Assembly
            Configuration.GetSection("ApplicationParts:AssemblyNames")
                .AsEnumerable()
                .Select(t => t.Value)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList()
                .ForEach(t => builder.AddApplicationPart(Assembly.Load(new AssemblyName(t))));

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "TNC Services API Platform", Version = "v1" });
            });

            //// config authentication based on environment
            //var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            //if (environment == EnvironmentName.Development)
            //{
            services.AddMvc();
            //}
            //else
            //{
            //    // Add authentication settings 
            //    services.AddAuthentication(sharedOptions =>
            //    {
            //        sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //    })
            //    .AddAzureAdBearer(options =>
            //        (new ConfigurationBuilder()
            //            .AddJsonFile(Path.Combine(assemblyFolderPath, "secret.json"))
            //            .Build())
            //        .Bind("AzureAd", options));

            //    services.AddMvc(o =>
            //    {
            //        var policy = new AuthorizationPolicyBuilder()
            //            .RequireAuthenticatedUser()
            //            .Build();
            //        o.Filters.Add(new AuthorizeFilter(policy));
            //    });
            //}

            // Dependency Injection
            services.AddSingleton<IKeyVaultAccessModel>(sp => new KeyVaultAccessModel(Path.Combine(assemblyFolderPath, "secret.json"), Path.Combine(assemblyFolderPath, "secretmap.json")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TNC Services API Platform");
            });

            //if (!env.IsDevelopment())
            //{
            //    app.UseAuthentication();
            //}

            app.UseMvc();
        }

    }
}
