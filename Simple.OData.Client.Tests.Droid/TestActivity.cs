using System.Reflection;
using Android.App;

namespace MonoDroidUnitTesting
{
    [Activity(Label = "Simple.OData.Client Droid Tests", MainLauncher = true, Icon = "@drawable/icon")]
    public class TestActivity : GuiTestRunnerActivity
    {
        protected override TestRunner CreateTestRunner()
        {
            var runner = new TestRunner();
            // Run all tests from this assembly
            runner.AddTests(Assembly.GetExecutingAssembly());
            return runner;
        }
    }

}

