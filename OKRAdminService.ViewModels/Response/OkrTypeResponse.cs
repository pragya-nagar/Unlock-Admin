using System;
using System.Collections.Generic;
using System.Text;

namespace OKRAdminService.ViewModels.Response
{
    public class OkrTypeResponse
    {
        public  int Id { get; set; }
        

        public  string StatusName { get; set; }
       
        public  string Description { get; set; }

        public  string Code { get; set; }


        public  string Color { get; set; }


        public  bool IsActive { get; set; }

    }
}
