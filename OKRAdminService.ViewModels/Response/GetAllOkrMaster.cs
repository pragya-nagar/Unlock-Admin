using System;
using System.Collections.Generic;
using System.Text;

namespace OKRAdminService.ViewModels.Response
{
   public class GetAllOkrMaster
    {
        public List<GoalTypeResponse> GoalTypes { get; set; }
        public List<GoalStatusResponse> GoalStatus { get; set; }

        public List<KrStatusResponse> KrStatus { get; set; }

        public List<MetricMasterResponse> MetricMasters { get; set; }

        public List<AssignmentTypeResponse> AssignmentTypes { get; set; }

        public List<OkrTypeResponse> okrTypes { get; set; }


        public List<DirectReporteesResponse> directReportees { get; set; }

    }

}
