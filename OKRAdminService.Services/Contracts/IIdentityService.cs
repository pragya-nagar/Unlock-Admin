using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OKRAdminService.ViewModels;

namespace OKRAdminService.Services.Contracts
{
    public interface IIdentityService
    {
        Identity GetUser(string emailId);
    }
}
