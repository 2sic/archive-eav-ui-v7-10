using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence
{
    public class HelpersToRefactor
    {
        /// <summary>
        /// Convert IOrderedDictionary to <see cref="Dictionary{String, ValueViewModel}" /> (for backward capability)
        /// </summary>
        public static Dictionary<string, ValueViewModel> DictionaryToValuesViewModel(IDictionary newValues)
        {
            if (newValues is Dictionary<string, ValueViewModel>)
                return (Dictionary<string, ValueViewModel>)newValues;

            return newValues.Keys.Cast<object>().ToDictionary(key => key.ToString(), key => new ValueViewModel { ReadOnly = false, Value = newValues[key] });
        }
    }
}
