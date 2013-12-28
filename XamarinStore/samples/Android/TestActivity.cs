using System.Reflection;
using Android.App;
using Android.OS;
using Xamarin.Android.NUnitLite;

namespace Simple.OData.Client.Tests.Android
{
	[Activity(Label = "Simple.OData.Client Android Tests", MainLauncher = true, Icon = "@drawable/icon")]
	public class TestActivity : TestSuiteActivity
    {
		protected override void OnCreate (Bundle bundle)
		{
			// tests can be inside the main assembly
			AddTest (Assembly.GetExecutingAssembly ());
			// or in any reference assemblies
			// AddTest (typeof (Your.Library.TestClass).Assembly);

			// Once you called base.OnCreate(), you cannot add more assemblies.
			base.OnCreate (bundle);
		}
    }

}

