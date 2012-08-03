using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using System;
    using System.ServiceModel.Web;
    using System.Threading;

    public class TestService : IDisposable
    {
        private WebServiceHost _host;
        private Uri _serviceUri;
        private static int _lastHostId = 1;

        public TestService(Type serviceType)
        {
            for (int i = 0; i < 100; i++)
            {
                int hostId = Interlocked.Increment(ref _lastHostId);
                this._serviceUri = new Uri("http://" + Environment.MachineName + "/Temporary_Listen_Addresses/SimpleODataTestService" + hostId.ToString() + "/");
                this._host = new WebServiceHost(serviceType, this._serviceUri);
                try
                {
                    this._host.Open();
                    break;
                }
                catch (Exception)
                {
                    this._host.Abort();
                    this._host = null;
                }
            }

            if (this._host == null)
            {
                throw new InvalidOperationException("Could not open a service even after 100 tries.");
            }
        }

        public void Dispose()
        {
            if (this._host != null)
            {
                this._host.Close();
                this._host = null;
            }
        }

        public Uri ServiceUri
        {
            get { return this._serviceUri; }
        }
    }
}
