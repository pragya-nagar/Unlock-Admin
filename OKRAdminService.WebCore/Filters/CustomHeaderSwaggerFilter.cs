
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace OKRAdminService.WebCore.Filters
{
    public class CustomHeaderSwaggerFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
    {
        public static IWebHostEnvironment AppEnvironment { get; private set; }
        public IConfiguration Configuration { get; }
        public CustomHeaderSwaggerFilter(IWebHostEnvironment env, IConfiguration configuration)
        {
            AppEnvironment = env;
            Configuration = configuration;
        }
        public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
        {
            operation.Parameters ??= new System.Collections.Generic.List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Token",
                In = ParameterLocation.Header,
                Description = "access token",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString("Bearer "),
                }
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "TenantId",
                In = ParameterLocation.Header,
                Description = "Domain Tenant",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString(string.Empty),
                }
            });
        }
    }
}
