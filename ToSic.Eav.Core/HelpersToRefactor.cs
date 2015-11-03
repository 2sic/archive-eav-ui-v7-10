using System;
using System.Globalization;
using ToSic.Eav.Data;

namespace ToSic.Eav
{
    public class HelpersToRefactor
    {
        /// <summary>
        /// Serialize Value to a String for SQL Server or XML Export
        /// </summary>
        public static string SerializeValue(object newValue)
        {
            string newValueSerialized;
            if (newValue is DateTime)
                newValueSerialized = ((DateTime)newValue).ToString("s");
            else if (newValue is double)
                newValueSerialized = ((double)newValue).ToString(CultureInfo.InvariantCulture);
            else if (newValue is decimal)
                newValueSerialized = ((decimal)newValue).ToString(CultureInfo.InvariantCulture);
            else if (newValue == null)
                newValueSerialized = string.Empty;
            else
                newValueSerialized = newValue.ToString();
            return newValueSerialized;
        }


    }
}