using System.Data.Entity.Infrastructure;
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;
using WebApiOData.V3.Samples.Models;

namespace WebApiOData.V3.Samples.Controllers;

public class NonBindableActionsController : ODataController
{
	private readonly MoviesContext db = new();

	[HttpPost]
	public Movie CreateMovie(ODataActionParameters parameters)
	{
		if (!ModelState.IsValid)
		{
			throw new HttpResponseException(HttpStatusCode.BadRequest);
		}

		var title = parameters["Title"] as string;

		var movie = new Movie()
		{
			Title = title
		};

		try
		{
			db.Movies.Add(movie);
			db.SaveChanges();
		}
		catch (DbUpdateConcurrencyException)
		{
			throw new HttpResponseException(HttpStatusCode.BadRequest);
		}

		return movie;
	}

	protected override void Dispose(bool disposing)
	{
		db.Dispose();
		base.Dispose(disposing);
	}
}
