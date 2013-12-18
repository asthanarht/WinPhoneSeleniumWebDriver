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
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace WindowsPhoneDriver
{
    public class WebRequest
    {
        public Dictionary<WebRequestHeader, string> Headers;

        public uint ContentLength
        {
            get
            {
                if (this.Headers.ContainsKey(WebRequestHeader.ContentLength))
                {
                    return uint.Parse(this.Headers[WebRequestHeader.ContentLength]);
                }
                else
                {
                    return 0;
                }
            }
        }

        public enum WebRequestMethod
        {
            Get,
            Post,
            Put,
            Delete
        }

        public enum WebRequestHeader
        {
            ContentType,
            ContentLength,
            Cookie,
            Accept,
            Host
        }

        public WebRequestMethod Method;
        public string ResurcePath;
        public string Content;
        public string HttpVerison;

        public Dictionary<string, object> DeserializeContent()
        {
            return WebDriverWireProtocolJsonConverter.Deserialize(this.Content);
        }
    }
}
