using System.ComponentModel.DataAnnotations.Schema;

namespace OKRAdminService.EF
{
    public class EntityBase : IObjectState
    {
        [NotMapped]
        public ObjectState ObjectStateEnum { get; set; }
    }
}
