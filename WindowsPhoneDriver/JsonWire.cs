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
using System.Windows.Threading;

namespace WindowsPhoneDriver
{
    public class JsonWire
    {
        public string UnknownCommandMessage = "Command could not be mapped to appropriate REST resource, check your session Id.";

        public enum ResponseCode
        { 
            Sucess = 0,
            NoSuchDriver = 6,
            NoSuchElement = 7,
            NoSuchFrame = 8,
            UnknownCommand = 9,
            StaleElementReference = 10,
            ElementNotVisible = 11,
            InvalidElementState = 12,
            UnknownError = 13,
            ElementIsNotSelectable = 15,
            JavaScriptError = 17,
            XPathLookupError = 19,
            Timeout = 21,
            NoSuchWindow = 23,
            InvalidCookieDomain = 24,
            UnableToSetCookie = 25,
            UnexpectedAlertOpen = 26,
            NoAlertOpenError = 27,
            ScriptTimeout = 28,
            InvalidElementCoordinates = 29, 
            IMENotAvailable = 30,
            IMEEngineActivationFailed = 31,
            InvalidSelector = 32,
            SessionNotCreatedException = 33,
            MoveTargetOutOfBounds = 34
        }

        public string GetBrowserCapabilities(string sessionId)
        { 
            return string.Format("{{0}:{1},{2}:{3}}",
                Helpers.ToJsonString("sessionId"),
                Helpers.ToJsonString(sessionId),
                Helpers.ToJsonString("status"),
                Helpers.ToJsonString("0"));
        }

        public static string BuildRespose(string valueJson, JsonWire.ResponseCode code, string sessionId)
        {
            return string.Format("{{\"sessionId\":\"{0}\", \"status\":\"{1}\", \"value\":{2}}}",
                sessionId,
                (int)code,
                valueJson);
        }
    }
}
