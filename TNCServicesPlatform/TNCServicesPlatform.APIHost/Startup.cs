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
using Microsoft.Azure.KeyVault.Models;

namespace TNCServicesPlatform.APIHost
{
    public class Startup
    {
        public static KeyVaultClient kv = new KeyVaultClient(KeyVaultAuthenticationCallback.Value);
        private static Lazy<AuthenticationCallback>  KeyVaultAuthenticationCallback => new Lazy<AuthenticationCallback>(() => new AuthenticationCallback(GetToken));
        private static Lazy<IConfiguration> _secretConfiguration = new Lazy<IConfiguration>(() => new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("secret.json").Build());
        private static Lazy<IConfiguration> _secretMapConfiguration = new Lazy<IConfiguration>(() => new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("secretmap.json").Build());

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
            var kvClientId = _secretConfiguration.Value.GetSection("KeyVaultAuth:ClientId").Value;
            var kvClientSecret = _secretConfiguration.Value.GetSection("KeyVaultAuth:ClientSecret").Value;
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(kvClientId, kvClientSecret);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        public static async Task<SecretBundle> GetKeyByName(string name)
        {
            var testx = _secretMapConfiguration.Value;

            var kv = new KeyVaultClient(GetToken);
            var secT = await kv.GetSecretAsync(_secretMapConfiguration.Value.GetSection("KeyVaultAuth:KeyNameUrlMap:"+name).Value);
            return secT;
        }
    }
}
