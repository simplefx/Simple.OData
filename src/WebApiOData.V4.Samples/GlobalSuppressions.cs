// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
	"Style",
	"IDE0060:Remove unused parameter",
	Justification = "Part of test design",
	Scope = "member",
	Target = "~M:WebApiOData.V4.Samples.Controllers.ProductsController.Placements(System.Int32,Microsoft.AspNet.OData.Query.ODataQueryOptions{WebApiOData.V4.Samples.Models.Movie})~System.Web.Http.IHttpActionResult")
]
