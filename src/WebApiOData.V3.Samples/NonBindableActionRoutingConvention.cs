using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;

namespace WebApiOData.V3.Samples;

public class NonBindableActionRoutingConvention : IODataRoutingConvention
{
	private readonly string _controllerName;

	public NonBindableActionRoutingConvention(string controllerName)
	{
		_controllerName = controllerName;
	}

	// Route all non-bindable actions to a single controller.
	public string SelectController(ODataPath odataPath, System.Net.Http.HttpRequestMessage request)
	{
		if (odataPath.PathTemplate == "~/action")
		{
			return _controllerName;
		}

		return null;
	}

	// Route the action to a method with the same name as the action.
	public string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
	{
		// OData actions must be invoked with HTTP POST.
		if (controllerContext.Request.Method == HttpMethod.Post)
		{
			if (odataPath.PathTemplate == "~/action")
			{
				var actionSegment = odataPath.Segments.First() as ActionPathSegment;
				var action = actionSegment.Action;

				if (!action.IsBindable && actionMap.Contains(action.Name))
				{
					return action.Name;
				}
			}
		}

		return null;
	}
}
