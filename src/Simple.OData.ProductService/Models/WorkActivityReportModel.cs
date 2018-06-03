using System;

// ReSharper disable CheckNamespace
namespace Simple.OData.ProductService.Models
// ReSharper restore CheckNamespace
{
    public class WorkActivityReportModel : BaseModel
    {
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public Guid Type { get; set; }
        public GeoLocationModel Location { get; set; }
        public Guid WorkTaskId { get; set; }
        public Guid WorkerId { get; set; }
    }
}
