using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services.Providers;

namespace ActionProviderImplementation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute: Attribute
    {
        public ActionAttribute()
        {
            this.Binding = OperationParameterBindingKind.Always;
        }
        public OperationParameterBindingKind Binding { get; protected set; }
        public string AvailabilityMethodName { get; protected set; }
    }
    public class OccasionallyBindableAction : ActionAttribute
    {
        public OccasionallyBindableAction(string availabilityMethod)  {
            this.AvailabilityMethodName = availabilityMethod;
            this.Binding = OperationParameterBindingKind.Sometimes;
        }
    }
    public class NonBindableAction : ActionAttribute
    {
        public NonBindableAction(){
            this.Binding = OperationParameterBindingKind.Never;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SkipCheckForFeeds : Attribute
    { 
    
    }
}
