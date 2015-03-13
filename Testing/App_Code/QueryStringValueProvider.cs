using System.Web;
using ToSic.Eav.ValueProvider;

public class QueryStringValueProvider : IValueProvider
{
	public string GetProperty(string propertyName, ref bool propertyNotFound)
	{
		var context = HttpContext.Current;

		if (context == null)
		{
			propertyNotFound = false;
			return null;
		}

		return context.Request.QueryString[propertyName.ToLower()];
	}

	public string Name { get { return "querystring"; } }

	public string Get(string property, string format, ref bool propertyNotFound)
	{
		return GetProperty(property, ref propertyNotFound);
	}
}