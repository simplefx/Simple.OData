using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.Services.Providers;
using System.Data.Services;
using System.Data.Entity;

namespace ActionProviderImplementation
{
    public class ActionInvokable : IDataServiceInvokable
    {
        ServiceAction _serviceAction;    
        Action _action;
        bool _hasRun = false;
        object _result;

        public ActionInvokable(DataServiceOperationContext operationContext, ServiceAction serviceAction, object site, object[] parameters, IParameterMarshaller marshaller)
        {
            _serviceAction = serviceAction;
            ActionInfo info = serviceAction.CustomState as ActionInfo;
            var marshalled = marshaller.Marshall(operationContext,serviceAction,parameters);

            info.AssertAvailable(site,marshalled[0], true);
            _action = () => CaptureResult(info.ActionMethod.Invoke(site, marshalled));
        }
        public void CaptureResult(object o)
        {
            if (_hasRun) throw new Exception("Invoke not available. This invokable has already been Invoked.");
            _hasRun = true;
            _result = o;
        }
        public object GetResult()
        {
            if (!_hasRun) throw new Exception("Results not available. This invokable hasn't been Invoked.");
            return _result;
        }
        public void Invoke()
        {
            try
            {
                _action();
            }
            catch {
                throw new DataServiceException(
                    500,
                    string.Format("Exception executing action {0}", _serviceAction.Name)
                );
            }
        }
    }
}
