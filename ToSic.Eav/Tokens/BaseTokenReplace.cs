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

//using System.Globalization;

//using System.Threading;

//using DotNetNuke.Common.Utilities;

#endregion

//namespace DotNetNuke.Services.Tokens
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ToSic.Eav.Tokens
{
	/// <summary>
	/// The BaseTokenReplace class provides the tokenization of tokens formatted  
	/// [object:property] or [object:property|format|ifEmpty] or [custom:no] within a string
	/// with the appropriate current property/custom values.
	/// </summary>
	/// <remarks></remarks>
	public abstract class BaseTokenReplace
	{
		//private const string ExpressionDefault = "(?:\\[(?:(?<object>[^\\]\\[:]+):(?<property>[^\\]\\[\\|]+))(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])|(?<text>\\[[^\\]\\[]+\\])|(?<text>[^\\]\\[]+)";
		private const string ExpressionDefault = @"(?:\[(?:(?<object>[^\]\[:]+):(?<property>[^\]\[\|]+))(?:\|(?:(?<format>[^\]\[]*)\|(?:(?<ifEmpty>[^\[\}]+)|(?:(?<ifEmpty>\[(?>[^\[\]]+|\[(?<number>)|\](?<-number>))*(?(number)(?!))\]))))|\|(?:(?<format>[^\|\]\[]+)))?\])|(?<text>\[[^\]\[]+\])|(?<text>[^\]\[]+)";

        //2dm 2015-03-09 testing...
        private const string maybeBetter = @"(?:\[(?:(?<object>[^\]\[:]+):(?<property>[^\]\[\|]+))(?:\|(?:(?<format>[^\]\[]*)\|(?:(?<ifEmpty>[^\[\}]+)|(?:(?<ifEmpty>\[(?>[^\[\]]+|\[(?<number>)|\](?<-number>))*(?(number)(?!))\]))))|\|(?:(?<format>[^\|\]\[]+)))?\])|(?<text>\[[^\]\[]+\])|(?<text>[^\]\[]+)|(?<text>.*)";
        private const string evenBetterRequiringOptionIgnoreWhitespace = @"



# start by defining a group, but don't give it an own capture-name
(?:
# Every token must start with a square bracket
\[(?:
    # then get the object name, at least 1 char before a :, then followed by a :
    (?<object>[^\]\[:\s]+):
    # next get property key - can actually be very complex and include sub-properties; but it ends with a [,| or ]
    (?<property>[^\]\[\|\:\s]+))
    # there may be more, but it's optional
    (?:
        # an optional format-parameter, it would be initiated by an |
        \|(?:(?<format>[^\]\[]*)
        # followed by another optional if-empty param, except that the if-empty can be very complex, containing more tokens
        \|(?:
            (?<ifEmpty>[^\[\}]+)
            # if ifEmpty contains more tokens, count open/close to make sure they are balanced
            |(?:
                (?<ifEmpty>\[(?>[^\[\]]+|\[(?<number>)|\](?<-number>))*(?(number)(?!))\])))
            )
        # not sure where this starts - or what it's for, but it's an 'or after a | you find a format...
        |\|(?:(?<format>[^\|\]\[]+))
    )?   # this packages is allowed 0 or 1 times so it ends with a ?
# and of course such a token must end with a ]
\])

# DISABLED THIS as it seems to cause more trouble than anything else
# try to detect anything else because we need captures to keep the parts in between
# give all the 3 versions the name <text>
# don't clearly understand why they are needed, but at least 2 and 3 are necessary
#|(?<text>\[[^\]\[]+\])
#|(?<text1>[^\]\[]+)
#|(?<text2>[\s\S]+)
";

        private const string TestStringForExpresso = @"Select * From Users Where UserId = [QueryString:UserName||[AppSettings:DefaultUserName||0]] or UserId = [AppSettings:RootUserId] or UserId = [Parameters:TestKey:Subkey||27]

some tests [] shouldn't capture and [ ] shouldn't capture either, + [MyName] shouldn't either";

        private static readonly Regex _tokenizer = new Regex(evenBetterRequiringOptionIgnoreWhitespace, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
		/// <summary>
		/// Gets the Regular expression for the token to be replaced
		/// </summary>
		/// <value>A regular Expression</value>   
		public static Regex Tokenizer
		{
			get
			{
				return _tokenizer;
			}
		}

        //protected virtual string ReplaceTokens(string sourceText)
        public virtual string ReplaceTokens(string sourceText, int repeat = 0)
        {
            if (string.IsNullOrEmpty(sourceText))
                return string.Empty;

            var Result = new StringBuilder();
            var charProgress = 0;
            var matches = Tokenizer.Matches(sourceText);
            if (matches.Count > 0)
            {
                foreach (Match curMatch in Tokenizer.Matches(sourceText))
                {
                    // Get characters before the first match
                    if (curMatch.Index > charProgress)
                    {
                        Result.Append(sourceText.Substring(charProgress, curMatch.Index - charProgress));
                        charProgress = curMatch.Index + curMatch.Length;
                    }

                    string strObjectName = curMatch.Result("${object}");
                    if (!String.IsNullOrEmpty(strObjectName))
                    {
                        string strPropertyName = curMatch.Result("${property}");
                        string strFormat = curMatch.Result("${format}");
                        string strIfEmptyReplacment = curMatch.Result("${ifEmpty}");
                        string strConversion = replacedTokenValue(strObjectName, strPropertyName, strFormat);
                        if (!String.IsNullOrEmpty(strIfEmptyReplacment) && String.IsNullOrEmpty(strConversion))
                            strConversion = strIfEmptyReplacment;
                        Result.Append(strConversion);
                    }
                }

                // attach the rest of the text (after the last match)
                Result.Append(sourceText.Substring(charProgress));
                
                // Ready to finish, but first, ensure repeating if desired
                string finalResult = Result.ToString();
                if (repeat > 0)
                    finalResult= ReplaceTokens(finalResult, repeat - 1);
                return finalResult;
            }

            // no matches found, just return the sourceText
            return sourceText;
        }

		//protected virtual string ReplaceTokens(string sourceText)
	    public virtual string PreviousReplaceTokens(string strSourceText)
	    {
	        if (strSourceText == null)
	        {
	            return string.Empty;
	        }
	        var Result = new StringBuilder();
	        foreach (Match currentMatch in Tokenizer.Matches(strSourceText))
	        {
	            string strObjectName = currentMatch.Result("${object}");
	            if (!String.IsNullOrEmpty(strObjectName))
	            {
	                //if (strObjectName == "[")
	                //{
	                //	strObjectName = ObjectLessToken;
	                //}
	                string strPropertyName = currentMatch.Result("${property}");
	                string strFormat = currentMatch.Result("${format}");
	                string strIfEmptyReplacment = currentMatch.Result("${ifEmpty}");
	                string strConversion = replacedTokenValue(strObjectName, strPropertyName, strFormat);
	                if (!String.IsNullOrEmpty(strIfEmptyReplacment) && String.IsNullOrEmpty(strConversion))
	                {
	                    strConversion = strIfEmptyReplacment;
	                }
	                Result.Append(strConversion);
	            }
	            else
	            {
	                Result.Append(currentMatch.Result("${text}"));
	            }
	        }
	        return Result.ToString();
	    }

	    protected abstract string replacedTokenValue(string strObjectName, string strPropertyName, string strFormat);
	}
}
