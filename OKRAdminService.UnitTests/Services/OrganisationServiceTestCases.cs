using Moq;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;

namespace OKRAdminService.UnitTests.Services
{
    public class OrganisationServiceTestCases
    {
        private readonly Mock<IRepositoryAsync<Organisation>> _organisationRepo;
        private readonly Mock<IRepositoryAsync<OrganisationCycle>> _organisationCycleRepo;
        private readonly Mock<IRepositoryAsync<OrganizationObjective>> _organisationObjectiveRepo;
        private readonly Mock<IRepositoryAsync<Employee>> _employeeRepo;
        private readonly Mock<IRepositoryAsync<ObjectivesMaster>> _objectiveMasterRepo;
        private readonly Mock<IRepositoryAsync<CycleDurationSymbol>> _cycleDurationSymbolRepo;
        private readonly Mock<IRepositoryAsync<CycleDurationMaster>> _cycleDurationMasterRepo;
        private readonly Mock<IServicesAggregator> _servicesAggregator;
        private readonly Mock<IUnitOfWorkAsync> _unitOfWork;

        public OrganisationServiceTestCases()
        {
            _organisationRepo = new Mock<IRepositoryAsync<Organisation>>();
            _organisationCycleRepo = new Mock<IRepositoryAsync<OrganisationCycle>>();
            _organisationObjectiveRepo = new Mock<IRepositoryAsync<OrganizationObjective>>();
            _employeeRepo = new Mock<IRepositoryAsync<Employee>>();
            _objectiveMasterRepo = new Mock<IRepositoryAsync<ObjectivesMaster>>();
            _cycleDurationSymbolRepo = new Mock<IRepositoryAsync<CycleDurationSymbol>>();
            _cycleDurationMasterRepo = new Mock<IRepositoryAsync<CycleDurationMaster>>();
            _servicesAggregator = new Mock<IServicesAggregator>();
            _unitOfWork = new Mock<IUnitOfWorkAsync>();

            _unitOfWork.Setup(x => x.RepositoryAsync<Organisation>()).Returns(_organisationRepo.Object);
            _unitOfWork.Setup(x => x.RepositoryAsync<OrganisationCycle>()).Returns(_organisationCycleRepo.Object);
            _unitOfWork.Setup(x => x.RepositoryAsync<OrganizationObjective>()).Returns(_organisationObjectiveRepo.Object);
            _unitOfWork.Setup(x => x.RepositoryAsync<Employee>()).Returns(_employeeRepo.Object);
            _unitOfWork.Setup(x => x.RepositoryAsync<ObjectivesMaster>()).Returns(_objectiveMasterRepo.Object);
            _unitOfWork.Setup(x => x.RepositoryAsync<CycleDurationSymbol>()).Returns(_cycleDurationSymbolRepo.Object);
            _unitOfWork.Setup(x => x.RepositoryAsync<CycleDurationMaster>()).Returns(_cycleDurationMasterRepo.Object);
            _unitOfWork.Setup(x => x.RepositoryAsync<Employee>()).Returns(_employeeRepo.Object);
            _servicesAggregator.Setup(x => x.UnitOfWorkAsync).Returns(_unitOfWork.Object);            

        }
    }
}
