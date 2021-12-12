using System;

// ReSharper disable CheckNamespace
namespace Simple.OData.ProductService.Models;

public class WorkTaskAttachmentModel : BaseModel
{
	public Guid Type { get; set; }
	public string FileName { get; set; }
	public Guid WorkTaskId { get; set; }
}
