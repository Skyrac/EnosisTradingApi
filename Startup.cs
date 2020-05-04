using API.Models;
using API.Repository;
using API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace API
{
    public class Startup
    {
        private const string corsPolicy = "_specifiedOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                                  builder =>
                                  {
                                      builder.WithOrigins("https://money-moon.web.app").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                                  });
            });
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0);
            services.AddAntiforgery();
            services.AddSwaggerDocument();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddEntityFrameworkCosmos();
            //services.AddDbContext<DatabaseEntitiesContext>(options => options.UseCosmos("https://money-moon-db-server.documents.azure.com:443/",
                                       // "jkIbnlwNqaxKzpYhIz0zbIMVWxHHi8CVMoktKNWSQaZqnzAVEYaq0sO3r2do9QKMiQHCHl9S5u2qz9Y9XeI1qA==",
                                       // databaseName: "MoneyMoonDb"));
            services.AddSingleton(InitializeCosmosClientInstanceAsync<UserTaskEntity>(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
            services.AddSingleton(InitializeCosmosClientInstanceAsync<UserEntity>(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
            services.AddSingleton(InitializeCosmosClientInstanceAsync<MineEntity>(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
            services.AddSingleton(InitializeCosmosClientInstanceAsync<LeadEntity>(Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
            services.AddScoped<IMine, MineService>();

            services.AddControllers();
        }

        private static async Task<ICosmosDatabase<T>> InitializeCosmosClientInstanceAsync<T>(IConfigurationSection configurationSection) where T : CosmoModel
        {
            string databaseName = configurationSection.GetSection("DatabaseName").Value;
            string containerName = typeof(T).Name; ;
            string account = "https://money-moon-db-server.documents.azure.com:443/";
            string key = configurationSection.GetSection("Key").Value;
            var client = new CosmosClient(account, key, new CosmosClientOptions() { AllowBulkExecution = true });
            var cosmosDbService = new CosmosDatabaseService<T>(client, databaseName, containerName);
            DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            return cosmosDbService;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseOpenApi();
            app.UseSwaggerUi3();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
