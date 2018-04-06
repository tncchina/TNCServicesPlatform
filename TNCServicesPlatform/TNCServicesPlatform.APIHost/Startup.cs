using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using System.Linq;
using TNCServicesPlatform.APIHost.DataModels;

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
            services.AddMvc();

            // Dependency Injection
            services.AddSingleton<KeyVaultAccessModel, KeyVaultAccessModel>();


            //services.AddAuthentication(sharedOptions =>
            //{
            //    sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            //})
            //.AddAzureAd(options => Configuration.Bind("AzureAd", options))
            //.AddCookie();

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
            app.UseAuthentication();
            app.UseMvc();
        }

    }
}
