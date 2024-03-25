namespace WebApiOData.V4.Samples.Models;

public class MoviesContext
{
	public List<Movie> Movies { get; set; } =
			[
				new() { ID=1, Title = "Maximum Payback", Year = 1990 },
				new() { ID=2, Title = "Inferno of Retribution", Year = 2005 },
				new() { ID=3, Title = "Fatal Vengeance 2", Year = 2012 },
				new() { ID=4, Title = "Sudden Danger", Year = 2012 },
				new() { ID=5, Title = "Beyond Outrage", Year = 2014 },
				new() { ID=6, Title = "The Nut Job", Year = 2014 }
			];
}
