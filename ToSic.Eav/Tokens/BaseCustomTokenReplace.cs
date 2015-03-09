#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

#region Usings

//using System;
//using System.Text;

//using DotNetNuke.Entities.Users;

#endregion

//namespace DotNetNuke.Services.Tokens
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ToSic.Eav.PropertyAccess;

namespace ToSic.Eav.Tokens
{
	/// <summary>
	/// BaseCustomTokenReplace  allows to add multiple sources implementing <see cref="IPropertyAccess">IPropertyAccess</see>
	/// </summary>
	public abstract class BaseCustomTokenReplace : BaseTokenReplace
	{
		protected Dictionary<string, IPropertyAccess> PropertySource;

		protected override string replacedTokenValue(string strObjectName, string strPropertyName, string strFormat)
		{
			var result = string.Empty;
			var propertyNotFound = false;
			if (PropertySource.ContainsKey(strObjectName.ToLower()))
			{
				result = PropertySource[strObjectName.ToLower()].GetProperty(strPropertyName, strFormat, ref propertyNotFound);
			}
			return result;
		}

		#region "Public Methods"

		/// <summary>
		/// Checks for present [Object:Property] tokens
		/// </summary>
		/// <param name="strSourceText">String with [Object:Property] tokens</param>
		/// <returns></returns>
		/// <history>
		///    08/10/2007 [sleupold] created
		///    10/19/2007 [sleupold] corrected to ignore unchanged text returned (issue DNN-6526)
		/// </history>
		public bool ContainsTokens(string strSourceText)
		{
			if (!string.IsNullOrEmpty(strSourceText))
			{
				foreach (Match currentMatch in TokenizerRegex.Matches(strSourceText))
					if (currentMatch.Result("${object}").Length > 0)
						return true;
			}
			return false;
		}

		#endregion
	}
}
