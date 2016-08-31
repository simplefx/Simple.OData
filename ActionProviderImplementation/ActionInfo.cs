using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.Entity;
using System.Data.Services;
using System.Data.Services.Providers;

namespace ActionProviderImplementation
{
    public class ActionInfo
    {
        public ActionInfo(MethodInfo actionMethod, OperationParameterBindingKind bindable, string availabilityMethodName) 
        {
            this.ActionMethod = actionMethod;
            this.Binding = bindable;

            if ((this.Binding == OperationParameterBindingKind.Sometimes))
            {
                this.AvailabilityCheckMethod = GetAvailabilityMethod(availabilityMethodName);
                if (this.AvailabilityCheckMethod.GetCustomAttributes(typeof(SkipCheckForFeeds), true).Length != 0)
                    this.SkipAvailabilityCheckForFeeds = true;
            }
            else 
            {
                if (availabilityMethodName != null) 
                    throw new Exception("Unexpected availabilityMethodName provided.");
            }  
        }

        public MethodInfo ActionMethod { get; private set;}

        public OperationParameterBindingKind Binding { get; private set; }

        public void AssertAvailable(object context, object entity, bool inFeedContext)
        {
            if (entity == null)
                return; 
            if (!IsAvailable(context, entity, inFeedContext))
                throw new DataServiceException(404, "Action not found.");
        }

        public bool IsAvailable(object context, object entity, bool inFeedContext)
        {
            if (this.Binding == OperationParameterBindingKind.Always)
                return true;
            else if (this.Binding == OperationParameterBindingKind.Never)
                return true; // TODO: need a way to verify we are NOT being bound... although I think this is impossible.
            else if (inFeedContext && this.SkipAvailabilityCheckForFeeds)
                return true;
            else
                return (bool)AvailabilityCheckMethod.Invoke(context, new object[] { entity });
        }

        private MethodInfo AvailabilityCheckMethod { get; set; }
        private bool SkipAvailabilityCheckForFeeds { get; set; }
        private MethodInfo GetAvailabilityMethod(string availabilityMethodName)
        {
            if (availabilityMethodName == null)
                throw new Exception("If the action is conditionally available you need to provide a method to calculate availability");

            var declaringType = ActionMethod.DeclaringType;
            var method = declaringType.GetMethod(availabilityMethodName);

            if (method == null)
                throw new Exception(string.Format("Availability Method {0} was not found on type {1}", availabilityMethodName, declaringType.FullName));

            if (method.ReturnType != typeof(bool))
                throw new Exception(string.Format("AvailabilityCheck method ({0}) MUST return bool.", availabilityMethodName));

            var actionBindingParameterType = ActionMethod.GetParameters().First().ParameterType;
            var methodParameters = method.GetParameters();
            if (methodParameters.Count() != 1 || methodParameters.First().ParameterType != actionBindingParameterType)
                throw new Exception(string.Format("AvailabilityCheck method was expected to have this signature 'bool {0}({1})'", availabilityMethodName, actionBindingParameterType.FullName));

            return method;
        }
    }
}
