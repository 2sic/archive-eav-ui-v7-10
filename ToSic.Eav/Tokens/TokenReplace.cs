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

//using System.Collections;

//using System.Data;
//using System.Linq;
//using System.Web;

//using DotNetNuke.Common.Utilities;
//using DotNetNuke.Entities.Controllers;
//using DotNetNuke.Entities.Host;
//using DotNetNuke.Entities.Modules;
//using DotNetNuke.Entities.Portals;
//using DotNetNuke.Entities.Users;

#endregion

//namespace DotNetNuke.Services.Tokens
using System.Collections.Generic;
using ToSic.Eav.PropertyAccess;

namespace ToSic.Eav.Tokens
{
	/// <summary>
	/// The TokenReplace class provides the option to replace tokens formatted
	/// [object:property] or [object:property|format] or [custom:no] within a string
	/// with the appropriate current property/custom values.
	/// Example for Newsletter: 'Dear [user:Displayname],' ==> 'Dear Superuser Account,'
	/// Supported Token Sources: User, Host, Portal, Tab, Module, Membership, Profile,
	///                          Row, Date, Ticks, ArrayList (Custom), IDictionary
	/// </summary>
	public class TokenReplace : BaseCustomTokenReplace
	{
		/// <summary>
		/// Constructs a new TokenReplace
		/// </summary>
		/// <param name="propertySource"></param>
		public TokenReplace(Dictionary<string, IPropertyAccess> propertySource)
		{
			PropertySource = propertySource;
		}
	}
}
