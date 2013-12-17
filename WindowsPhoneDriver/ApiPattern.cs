//---------------------------------------------------------------------------------------------------------------------------------------------------
// Copyright Microsoft Corporation, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.
//---------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace WindowsPhoneDriver
{

    /// <summary>
    /// Class representing REST resource patterns. With this class you can check if
    /// given string is matching pattern and at the same time extract desired varaibles
    /// from string.
    /// </summary>
    public class ApiPattern
    {

        public WebRequest.WebRequestMethod Method { get; private set; }
        public Token[] Tokens;
        static public ApiPattern Empty { get { return null; } }

        /// <summary>
        /// Class that represent token of pattern (variable or constant)
        /// </summary>
        /// 

        public enum TokenType
        {
            Constant,
            Variable
        }

        public class Token
        {
            public string Value;
            public TokenType Type;

            public Token(string text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    throw new Exception("Null text string in token");
                }

                if (text[0] == ':')
                {
                    Type = TokenType.Variable;
                    Value = text.Substring(1);

                    if (string.IsNullOrEmpty(Value))
                    {
                        throw new Exception("Empty variable name");
                    }
                }
                else
                {
                    Type = TokenType.Constant;
                    Value = text;
                }
            }
        }

        /// <summary>
        /// Construtor for API pattern.
        /// </summary>
        /// <param name="method">HTTP method to match (GET, PUT, POST, DELETE)</param>
        /// <param name="pattern">Resource pattern string. All names prefixed with ':' will be treated as variables
        /// for example: /user/:id/name/change/:newname
        /// </param>
        public ApiPattern(WebRequest.WebRequestMethod method, string pattern)
        {
            string[] splits = pattern.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            Method = method;

            List<Token> tokensList = new List<Token>();

            foreach (string split in splits)
            {
                tokensList.Add(new Token(split));
            }

            Tokens = tokensList.ToArray();
        }

        /// <summary>
        /// Funtion will check if given WebRequest matches the pattern.
        /// </summary>
        /// <param name="pattern">Pattern.</param>
        /// <param name="request">Request to check.</param>
        /// <param name="matches">Dictionary containing values of variables defined in pattern.</param>
        /// <returns></returns>
        public static bool Match(ApiPattern pattern, WebRequest request, out Dictionary<string, string> matches)
        {
            matches = new Dictionary<string, string>();

            if (pattern.Method != request.Method)
            {
                return false;
            }

            string[] splits = request.ResurcePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (splits.Length != pattern.Tokens.Length)
            {
                return false;
            }

            for (int i = 0; i < pattern.Tokens.Length; i++)
            {
                switch (pattern.Tokens[i].Type)
                {
                    case TokenType.Variable:
                        matches[pattern.Tokens[i].Value] = splits[i];
                        break;
                    case TokenType.Constant:
                        if (String.Compare(pattern.Tokens[i].Value, splits[i], StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Overload for class instances.
        /// </summary>
        /// <param name="request">Request to check.</param>
        /// <param name="matches">Dictionary containing values of variables defined in pattern.</param>
        /// <returns></returns>
        public bool Match(WebRequest request, out Dictionary<string, string> matches)
        {
            return Match(this, request, out matches);
        }
    }
}
