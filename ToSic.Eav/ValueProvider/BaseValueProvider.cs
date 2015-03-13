using System.Text.RegularExpressions;

namespace ToSic.Eav.ValueProvider
{
	/// <summary>
	/// Property Accessor to test a Pipeline with Static Values
	/// </summary>
	public abstract class BaseValueProvider : IValueProvider
    {
        #region default methods of interface
        // note: set should be private, but hard to define through an interface
        public string Name { get; set; }

        // this is needed by some property accesses which support sub-properties like Content:Publisher:Location:City...
        // todo: should optimize to use named matches, to ensure that reg-ex changes doesn't change numbering...
        internal static readonly Regex SubProperties = new Regex("([a-z]+):([a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Default Get... must be overridden
        /// </summary>
        /// <param name="property"></param>
        /// <param name="format"></param>
        /// <param name="propertyNotFound"></param>
        /// <returns></returns>
	    public abstract string Get(string property, string format, ref bool propertyNotFound);

        /// <summary>
        /// Shorthand version, will return the string value or a null if not found. 
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
	    public virtual string Get(string property)
	    {
	        bool temp = false;
	        return Get(property, "", ref temp);
	    }

	    public abstract bool Has(string property);
        #endregion

        #region Helper functions
        /// <summary>
        /// Returns a formatted String if a format is given, otherwise it returns the unchanged value.
        /// </summary>
        /// <param name="value">string to be formatted</param>
        /// <param name="format">format specification</param>
        /// <returns>formatted string</returns>
        /// <remarks></remarks>
        public string FormatString(string value, string format)
        {
            if (string.IsNullOrWhiteSpace(format))// format.Trim() == string.Empty)
            {
                return value;
            }
            else if (string.IsNullOrEmpty(value))// != string.Empty)
            {
                return string.Format(format, value);
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
