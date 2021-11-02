using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OKRAdminService.EF;
using OKRAdminService.Services;
using OKRAdminService.Services.AutoMapper;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels.Response;
using OKRAdminService.WebCore.Filters;
using OKRAdminService.WebCore.Middleware;
using Polly;
using Serilog;
using Serilog.Events;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OKRAdminService.Common;


namespace OKRAdminService.WebCore
{
    public class Startup
    {
        public static IWebHostEnvironment AppEnvironment { get; private set; }
        public IConfiguration Configuration { get; }
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            AppEnvironment = env;
            Configuration = configuration;
            var envName = env?.EnvironmentName;
            Console.WriteLine("envName: " + envName);
            Log.Logger = new LoggerConfiguration()
               .Enrich.WithProperty("Environment", envName)
               .Enrich.WithMachineName()
               .Enrich.WithProcessId()
               .Enrich.WithThreadId()
               .WriteTo.Console()
               .MinimumLevel.Information()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
               .Enrich.FromLogContext()
               .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddLogging();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetValue<string>("Redis:ConnectionString");
                options.InstanceName = Configuration.GetValue<string>("Redis:InstanceName");
            });
            var keyVault = new DatabaseVaultResponse();
            services.AddDbContext<OkrAdminDbContext>((serviceProvider, options) =>
            {
                var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
                if (httpContextAccessor != null)
                {
                    var httpContext = httpContextAccessor.HttpContext;
                    var tokenDecoded = TokenDecoded(httpContext);
                    //var tid = tokenDecoded.Claims.FirstOrDefault(c => c.Type == "tid");
                    //var tenantId = tid == null ? string.Empty : tid.Value;
                    //var email = tokenDecoded.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
                    // Added comment for testing
                    var hasTenant = httpContext.Request.Headers.TryGetValue("TenantId", out var tenantId);
                    if ((!hasTenant && httpContext.Request.Host.Value.Contains("localhost")))
                        tenantId = Configuration.GetValue<string>("TenantId");
 
                    if (!string.IsNullOrEmpty(tenantId))
                    {
                        var tenantString = CryptoFunctions.DecryptRijndael(tenantId, Configuration.GetValue<string>("PrivateKey"));
                        var key = tenantString + "-Connection" + Configuration.GetValue<string>("KeyVaultConfig:PostFix");
                        keyVault.ConnectionString = Configuration.GetValue<string>(key);
                        var retryPolicy = Policy.Handle<Exception>().Retry(2, (ex, count, context) =>
                        {
                            (Configuration as IConfigurationRoot)?.Reload();
                            keyVault.ConnectionString = Configuration.GetValue<string>(key);
                        });
                        retryPolicy.Execute(() =>
                        {
                            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).UseSqlServer(keyVault?.ConnectionString);
                        });
                    }
                    else
                    {
                        Console.WriteLine("Invalid tenant is received");
                    }
                }
                //var conn = Configuration.GetConnectionString("ConnectionString");
                //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).UseSqlServer(conn);
            });

            services.AddScoped<IOperationStatus, OperationStatus>();
            services.AddScoped<IDataContextAsync>(opt => opt.GetRequiredService<OkrAdminDbContext>());
            services.AddScoped<IUnitOfWorkAsync, UnitOfWork>();
            services.AddTransient<IServicesAggregator, ServicesAggregator>();

            services.AddAutoMapper(Assembly.Load("OKRAdminService.Services"));
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
            services.AddAuthentication();
            services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)))
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSwaggerGen(c =>
                    {
                        c.OperationFilter<CustomHeaderSwaggerFilter>();
                        c.SwaggerDoc("v1", new OpenApiInfo()
                        {
                            Version = "v1",
                            Title = "OKR Admin APIs",
                            Description = "OKR Admin APIs",
                            TermsOfService = new Uri(Configuration.GetSection("TermsAndConditionUrl").Value)
                        });
                    });


            services.AddTransient<IPermissionService, PermissionService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IOrganisationService, OrganisationService>();
            services.AddTransient<IMasterService, MasterService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<INotificationsEmailsService, NotificationsEmailsService>();
            services.AddTransient<IPassportSsoService, PassportSsoService>();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IKeyVaultService, KeyVaultService>();
            services.AddTransient<TokenManagerMiddleware>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(Configuration.GetSection("SwaggerEndpoint").Value, "OKR Admin APIs");
            });
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthentication();
            //app.UseAuthorization();
            app.UseMiddleware<TokenManagerMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        #region PRIVATE METHODS

        private async Task<TenantResponse> GetTenantIdAsync(string domain, string email, string jwtToken)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(Configuration.GetValue<string>("TenantService:BaseUrl"))
            };
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await httpClient.GetAsync($"GetTenant?subDomain=" + domain + "&emailId=" + email);
            if (!response.IsSuccessStatusCode) return null;
            var payload = JsonConvert.DeserializeObject<PayloadCustom<TenantResponse>>(await response.Content.ReadAsStringAsync());
            return payload.Entity;
        }

        private async Task<DatabaseVaultResponse> KeyVaultConfiguration(string domainName, HttpContext httpContext)
        {
            Console.WriteLine("AppEnvironment" + AppEnvironment.EnvironmentName);

            Console.WriteLine("KeyVaultConfiguration Started");
            DatabaseVaultResponse databaseVault = new DatabaseVaultResponse();
            string authorization = httpContext.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorization))
                authorization = httpContext.Request.Headers["Token"];
            var token = string.Empty;
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = authorization.Substring("Bearer ".Length).Trim();

            string dbSecretApiUrl = Configuration.GetValue<string>("AzureSettings:AzureSecretApiUrl");

            Console.WriteLine("KeyVaultConfiguration dbSecretApiUrl: " + dbSecretApiUrl);

            var uri = new Uri(dbSecretApiUrl + domainName);
            Console.WriteLine("KeyVaultConfiguration uri: " + uri);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine("KeyVaultConfiguration client");

            var response = await client.GetAsync(uri);

            Console.WriteLine("KeyVaultConfiguration response: " + response);

            if (response is { Headers: { } })
            {
                var suffixName = Configuration.GetValue<string>("AzureSettings:Suffix");

                Console.WriteLine("KeyVaultConfiguration suffixName: " + suffixName);

                var conn = response.Headers.ToList().FirstOrDefault(x => x.Key == "ConnectionString" + suffixName);
                var schema = response.Headers.ToList().FirstOrDefault(x => x.Key == "Schema");

                Console.WriteLine("KeyVaultConfiguration conn.Key " + conn.Value.FirstOrDefault());
                Console.WriteLine("KeyVaultConfiguration  schema.Key " + schema.Value.FirstOrDefault());

                if (conn.Key != null && schema.Key != null)
                {
                    databaseVault.ConnectionString = conn.Value.FirstOrDefault();
                    databaseVault.CurrentSchema = schema.Value.FirstOrDefault();
                }
            }
            Console.WriteLine("KeyVaultConfiguration Finished");
            return databaseVault;
        }
        private JwtSecurityToken TokenDecoded(HttpContext context)
        {
            string authorization = context.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorization))
                authorization = context.Request.Headers["Token"];

            var token = string.Empty;
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = authorization.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
                return null;
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var tokenS = jsonToken as JwtSecurityToken;

            return tokenS;
        }

        #endregion

        
    }
}

