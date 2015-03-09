using System.Text.RegularExpressions;

namespace ToSic.Eav.PropertyAccess
{
	/// <summary>
	/// Property Accessor to test a Pipeline with Static Values
	/// </summary>
	public abstract class BasePropertyAccess : IPropertyAccess
    {
        #region default methods of interface
        // note: set should be private, but hard to define through an interface
        public string Name { get; set; }

        // this is needed by some property accesses which support sub-properties like Content:Publisher:Location:City...
        // todo: should optimize to use named matches, to ensure that reg-ex changes doesn't change numbering...
        internal static readonly Regex SubProperties = new Regex("([a-z]+):([a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Default GetProperty... must be overridden
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="format"></param>
        /// <param name="propertyNotFound"></param>
        /// <returns></returns>
	    public abstract string GetProperty(string propertyName, string format, ref bool propertyNotFound);
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
            if (format.Trim() == string.Empty)
            {
                return value;
            }
            else if (value != string.Empty)
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
