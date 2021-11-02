using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OKRAdminService.Services
{
    public class IdentityService : BaseService , IIdentityService
    {
        private readonly IRepositoryAsync<Employee> employeeRepo;
        public IdentityService(IServicesAggregator servicesAggregateService) : base(servicesAggregateService)
        {
            employeeRepo = UnitOfWorkAsync.RepositoryAsync<Employee>();
        }

        public Identity GetUser(string emailId)
        {
            var empDetail = employeeRepo.GetQueryable().FirstOrDefault(x=> x.EmailId == emailId && x.IsActive);
            var userModel = Mapper.Map<Identity>(empDetail);
            return userModel;
        }
    }
}
