using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;

namespace OKRAdminService.Services
{
    public class ServicesAggregator : IServicesAggregator
    {
        public IUnitOfWorkAsync UnitOfWorkAsync { get; set; }
        public IOperationStatus OperationStatus { get; set; }
        public IConfiguration Configuration { get; set; }
        public IMapper Mapper { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }

        public ServicesAggregator(IUnitOfWorkAsync unitOfWorkAsync, IOperationStatus operationStatus, IConfiguration configuration, IMapper mapper, IHostingEnvironment environment)
        {
            UnitOfWorkAsync = unitOfWorkAsync;
            OperationStatus = operationStatus;
            Configuration = configuration;
            HostingEnvironment = environment;
            Mapper = mapper;
        }
    }
}

