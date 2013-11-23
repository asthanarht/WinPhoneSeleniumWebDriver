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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPhoneDriver
{
    public class WebResponse
    {
        public enum StatusCodeType
        {
            Informational,
            Success,
            Redirection,
            ClientError,
            ServerError,
            Unknown
        }

        public static Dictionary<uint, string> ReasonPhrase = new Dictionary<uint, string>()
        {
            {100, "Continue"},
            {101, "Switching Protocols"},
            {200, "OK"},
            {201, "Created"},
            {202, "Acceped"},
            {203, "Non Authoritative Information"},
            {204, "No Content"},
            {205, "Reset Content"},
            {206, "Partial Content"},
            {300, "Multiple Choices"},
            {301, "Moved Permanently"},
            {302, "Found"},
            {303, "See Other"},
            {304, "Not Modified"},
            {305, "Use Proxy"},
            {307, "Temporary Redirect"},
            {400, "Bad Request"},
            {401, "Unauthorized"},
            {402, "Payment Required"},
            {403, "Forbidden"},
            {404, "Not Found"},
            {405, "Method Not Allowed"},
            {406, "Not Acceptable"},
            {407, "Proxy Authentication Required"},
            {408, "Request Time out"},
            {409, "Conflict"},
            {410, "Gone"},
            {411, "Length Required"},
            {412, "Precondition Failed"},
            {413, "Request Entity Too Large"},
            {414, "Request URI Too Large"},
            {415, "Unsupported Media Type"},
            {416, "Requested range not satisfiable"},
            {417, "Expectation Failed"},
            {500, "Internal Server Error"},
            {501, "Not Implemented"},
            {502, "Bad Gateway"},
            {503, "Service Unavailable"},
            {504, "Gateway Time out"},
            {505, "HTTP Version not supported"}
        };

        public string Content;
        public int ContentLength
        {
            get
            {
                return this.Content.Length;
            }
        }
        public string ContentType;
        public string Location;
        public uint StatusCode;
        private static string HTTPVersionString = "HTTP/1.1";

        public static WebResponse InternalServerError()
        {
            WebResponse response = new WebResponse();

            response.ContentType = "text/plan;charset=utf-8";
            response.StatusCode = 500;
            response.Content = "Call could not be fullfield. JavaScript injection failed.";

            return response;
        }

        public static WebResponse NotImplemented()
        {
            WebResponse response = new WebResponse();
            string msg = "Sorry, not implemented yet.";

            response.ContentType = "text/plan;charset=utf-8";
            response.StatusCode = 501;
            response.Content = msg;

            return response;
        }

        public static WebResponse SucessSimple()
        {
            WebResponse response = new WebResponse();

            response.StatusCode = 200;

            response.Content = string.Empty;

            return response;
        }

        override public string ToString()
        {
            switch (CodeType(this.StatusCode))
            {
                case StatusCodeType.Informational:
                    break;
                case StatusCodeType.Success:
                    return SuccessToString();
                case StatusCodeType.Redirection:
                    return RedirectionToString();
                case StatusCodeType.ClientError:
                    return ClientErrorToString();
                case StatusCodeType.ServerError:
                    return ServerErrorToString();
                default:
                    throw new Exception("Wrong status code of response");
            }

            return string.Empty;
        }

        private string ClientErrorToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append(GetStatusLine());
            str.Append(GetServerLine());
            str.Append("Content-Type: ");
            str.Append(this.ContentType);
            str.Append("Content-Lenght: ");
            str.Append(ContentLength);
            str.AppendLine();
            str.AppendLine();
            str.Append(Content);

            return str.ToString();
        }

        private string RedirectionToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append(GetStatusLine());
            str.Append(GetServerLine());
            str.AppendLine("Content-Type: text/html; charset=utf-8");
            str.AppendLine("Content-Lenght: 0");
            str.AppendLine("Location: " + this.Location);
            str.AppendLine();

            return str.ToString();
        }

        private string SuccessToString()
        {
            return this.ServerErrorToString();
        }

        private string ServerErrorToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append(GetStatusLine());
            str.Append(GetServerLine());
            str.Append("Content-Type: " + this.ContentType);
            str.AppendLine();
            str.Append("Content-Lenght: " + this.ContentLength);
            str.AppendLine();
            str.Append("Connection: close");
            str.AppendLine();
            str.Append("Vary: Accept-Charset, Accept-Encoding, Accept-Language, Accept");
            str.AppendLine();
            str.Append("Accept-Ranges: bytes");
            str.AppendLine();
            str.AppendLine();
            str.Append(this.Content);

            return str.ToString();
        }

        private string GetServerLine()
        {
            return "Server: Selenium Windows Phone WebDriver\r\n";
        }

        private string GetStatusLine()
        {
            return string.Format("{0} {1} {2}{3}",
                    WebResponse.HTTPVersionString,
                    this.StatusCode,
                    WebResponse.ReasonPhrase[this.StatusCode],
                    Environment.NewLine
                    );
        }

        public StatusCodeType CodeType(uint code)
        {
            if (code < 200)
                return StatusCodeType.Informational;

            if (code < 300)
                return StatusCodeType.Success;

            if (code < 400)
                return StatusCodeType.Redirection;

            if (code < 500)
                return StatusCodeType.ClientError;

            if (code < 600)
                return StatusCodeType.ServerError;

            return StatusCodeType.Unknown;
        }
    }
}
