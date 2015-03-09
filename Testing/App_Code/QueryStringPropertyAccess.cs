using System.Web;
using ToSic.Eav.PropertyAccess;

public class QueryStringPropertyAccess : IPropertyAccess
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

	public string GetProperty(string propertyName, string format, ref bool propertyNotFound)
	{
		return GetProperty(propertyName, ref propertyNotFound);
	}
}