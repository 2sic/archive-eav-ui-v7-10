using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ToSic.Eav.PropertyAccess
{
	/// <summary>
	/// Get Values from Assigned Entities
	/// </summary>
	public class EntityPropertyAccess : BasePropertyAccess
    {
        #region for internal regex etc.
        //private static readonly Regex SubProperties = new Regex("([a-z]+):([a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        #endregion

        private IEntity _entity;
	    private string[] dimensions = new string[] {""};

	    public EntityPropertyAccess()
	    {
	        
	    }

		/// <summary>
		/// Constructs a new AssignedEntity AttributePropertyAccess
		/// </summary>
		/// <param name="name">Name of the PropertyAccess, e.g. pipelinesettings</param>
		/// <param name="objectId">EntityGuid of the Entity to get assigned Entities of</param>
		/// <param name="metaDataSource">DataSource that provides MetaData</param>
        public EntityPropertyAccess(IEntity source, string name = "entity source without name")
		{
            _entity = source;
		    Name = name;
		}

        // todo: might need to clarify what language/culture the property is taken from in an entity
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, ref bool propertyNotFound)
        {
            // Return empty string if Entity is null
            if (_entity == null)
                return string.Empty;

            string outputFormat = format == string.Empty ? "g" : format;

            //bool propertyNotFound;
            object valueObject = _entity.GetBestValue(propertyName, dimensions, out propertyNotFound);

            if (!propertyNotFound && valueObject != null)
            {
                switch (valueObject.GetType().Name)
                {
                    case "String":
                        return FormatString((string)valueObject, format);
                    case "Boolean":
                        return ((bool)valueObject).ToString(formatProvider).ToLower();
                    case "DateTime":
                    case "Double":
                    case "Single":
                    case "Int32":
                    case "Int64":
                    case "Decimal":
                        return (((IFormattable)valueObject).ToString(outputFormat, formatProvider));
                    default:
                        return FormatString(valueObject.ToString(), format);
                }
            }
            else
            {
                #region Check for Navigation-Property (e.g. Manager:Name)
                if (propertyName.Contains(':'))
                {
                    //var propertyMatch = Regex.Match(propertyName, "([a-z]+):([a-z]+)", RegexOptions.IgnoreCase);
                    var propertyMatch = SubProperties.Match(propertyName);
                    if (propertyMatch.Success)
                    {
                        valueObject = _entity.GetBestValue(propertyMatch.Groups[1].Value, dimensions, out propertyNotFound);
                        if (!propertyNotFound && valueObject != null)
                        {
                            #region Handle child-Entity-Field (sorted list of related entities)
                            var relationshipList = valueObject as EntityRelationshipModel;
                            if (relationshipList != null)
                            {
                                if (!relationshipList.Any())
                                    return string.Empty;
                                else
                                    return new EntityPropertyAccess(relationshipList.First())
                                        .GetProperty(propertyMatch.Groups[2].Value, format, formatProvider, ref propertyNotFound);
                            }
                            #endregion
                        }
                    }
                }
                #endregion

                propertyNotFound = true;
                return string.Empty;
            }
        }


        public override string GetProperty(string propertyName, string format, ref bool PropertyNotFound)
        {
            return GetProperty(propertyName, format, System.Threading.Thread.CurrentThread.CurrentCulture, ref PropertyNotFound);
        }

	}
}