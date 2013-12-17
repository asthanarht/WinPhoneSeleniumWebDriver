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
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace WindowsPhoneDriver
{
    public class Config
    {
        public UInt16 Port = 8080;
        public string ReturnIP = null;
        public int? ReturnPort = null;

        public Config(IsolatedStorageFileStream configFile)
        {
            var element = XElement.Load(configFile);

            var port = element.Element("Port");
            
            if (port != null)
            {
                this.Port = UInt16.Parse(port.Value);
            }

            var returnPort = element.Element("ReturnPort");
            if (returnPort != null)
            {
                this.ReturnPort = int.Parse(returnPort.Value);
            }

            var returnIP = element.Element("ReturnIP");
            if (returnIP != null)
            {
                this.ReturnIP = returnIP.Value;
            }
        }
    }
}
