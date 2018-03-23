using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using System.Linq;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using System;
using static Microsoft.Azure.KeyVault.KeyVaultClient;
using System.IO;

namespace TNCServicesPlatform.APIHost
{
    public class Startup
    {
        public static KeyVaultClient kv = new KeyVaultClient((new AuthenticationCallback(GetToken)));
        private static string kvName;
        private static string kvClientId;
        private static string kvClientSecret;


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
            kvName = Configuration.GetSection("KeyVaultAuth:Vault").Value;
            kvClientId = Configuration.GetSection("KeyVaultAuth:ClientId").Value;
            kvClientSecret = Configuration.GetSection("KeyVaultAuth:ClientSecret").Value;

            var key = GetKeyByName(@"WebSiteKey");

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "TNC Services API Platform", Version = "v1" });
            });
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

            app.UseMvc();
        }

        //the method that will be provided to the KeyVaultClient
        private static async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(kvClientId, kvClientSecret);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        public static string GetKeyByName(string name)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var Configuration = builder.Build();

            // I put my GetToken method in a Utils class. Change for wherever you placed your method.
            var kv = new KeyVaultClient(GetToken);
            var secT = kv.GetSecretAsync(Configuration.GetSection("KeyVaultAuth:KeyNameUrlMap:"+name).Value);
            Task.WaitAll(secT);
            return secT.Result.Value;
        }
    }
}
