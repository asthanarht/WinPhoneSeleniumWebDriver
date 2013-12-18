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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace WindowsPhoneDriver
{
    public class RequestHandlers
    {
        private AutomatedWebBrowser BrowserState;

        public RequestHandlers(AutomatedWebBrowser browserState)
        {
            this.BrowserState = browserState;
        }

        public WebResponse GetStatus(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> statusValue = new Dictionary<string, object>();
            Dictionary<string, object> build = new Dictionary<string, object>();
            build["version"] = "0.01";

            Dictionary<string, object> os = new Dictionary<string, object>();
            os["version"] = "8";
            os["name"] = "Windows Phone 8";

            statusValue["build"] = build;
            statusValue["os"] = os;

            WebResponse response = new WebResponse();

            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";
            response.Content = WebDriverWireProtocolJsonConverter.Serialize(statusValue);
            return response;
        }

        public WebResponse PostSession(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            WebResponse response = new WebResponse();
            StringBuilder location = new StringBuilder("/session/");
            string sessionId = BrowserState.CreateNewSession();
            location.Append(sessionId);
            
            //redirect user to new session
            response.StatusCode = 303;
            response.Location = location.ToString();

            return response;
        }

        public WebResponse GetSession(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            //this function is is just a stub
            //it dosen't care about session-id because we
            //currently don't support multiple sessions
            Dictionary<string, object> responseObject = BrowserState.DescribeSession();
            return ConstructResponse(responseObject);
        }

        public WebResponse DeleteSession(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> responseObject = BrowserState.EndSession();
            return ConstructResponse(responseObject);
        }

        public WebResponse PostTimeouts(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostTimeoutsAsyncScript(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostTimeoutsImplicitWait(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetWindowHandle(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> responseObject = BrowserState.GetCurrentWindowHandle();
            return ConstructResponse(responseObject);
        }

        public WebResponse GetWindowHandles(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> responseObject = BrowserState.GetAllWindowHandles();
            return ConstructResponse(responseObject);
        }

        public WebResponse GetUrl(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> responseObject = BrowserState.GetUrl();
            return ConstructResponse(responseObject);
        }

        public WebResponse PostUrl(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            List<string> missingParameters = GetMissingParameters(dict, new List<string>() { "url" });
            if (missingParameters.Count > 0)
            {
                return ConstructMissingParameterResponse(missingParameters);
            }

            Dictionary<string, object> responseObject = this.BrowserState.Navigate((string)dict["url"]);
            return ConstructResponse(responseObject);
        }

        public WebResponse PostForward(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> responseObject = this.BrowserState.GoForward();
            return ConstructResponse(responseObject);
        }

        public WebResponse PostBack(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> responseObject = this.BrowserState.GoBack();
            return ConstructResponse(responseObject);
        }

        public WebResponse PostRefresh(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> responseObject = this.BrowserState.Refresh();
            return ConstructResponse(responseObject);
        }

        public WebResponse PostExecute(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            var dict = request.DeserializeContent();
            List<string> missingParameters = GetMissingParameters(dict, new List<string>() { "script", "args" });
            if (missingParameters.Count > 0)
            {
                return ConstructMissingParameterResponse(missingParameters);
            }


            string script = dict["script"].ToString();
            string argsString = JsonConvert.SerializeObject(dict["args"]);

            Dictionary<string, object> ret = BrowserState.Execute(script, argsString);
            return ConstructResponse(ret);
        }

        public WebResponse PostExecuteAsync(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetScreenshot(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> responseObject = BrowserState.GetScreenshot();
            return ConstructResponse(responseObject);
        }

        public WebResponse GetImeAvailableEngines(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetImeActiveEngine(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetImeActivated(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostImeDeactivate(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostImeActivate(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostFrame(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            List<string> missingParameters = GetMissingParameters(dict, new List<string>() { "id" });
            if (missingParameters.Count > 0)
            {
                return ConstructMissingParameterResponse(missingParameters);
            }

            string id = null;
            if (dict["id"] != null)
            {
                id = dict["id"].ToString();
            }

            Dictionary<string, object> responseObject = BrowserState.SwitchToFrame(id);
            return ConstructResponse(responseObject);
        }

        public WebResponse PostWindow(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse DeleteWindow(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostWindowSize(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetWindowSize(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostWindowPosition(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetWindowPosition(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostWindowMaximize(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetCookie(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = this.BrowserState.GetCookies();
            return ConstructResponse(result);
        }

        public WebResponse PostCookie(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            List<string> missingParameters = GetMissingParameters(dict, new List<string>() { "cookie" });
            if (missingParameters.Count > 0)
            {
                return ConstructMissingParameterResponse(missingParameters);
            }

            Dictionary<string, object> cookie = dict["cookie"] as Dictionary<string, object>;
            Dictionary<string, object> result = BrowserState.AddCookie(cookie);
            return ConstructResponse(result);
        }

        public WebResponse DeleteCookie(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse DeleteCookieName(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetSource(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.GetSource();
            return ConstructResponse(result);
        }

        public WebResponse GetTitle(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.GetTitle();
            return ConstructResponse(result);
        }

        public WebResponse PostElement(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            Dictionary<string, object> result = BrowserState.FindElement((string)dict["using"], (string)dict["value"], null);
            return ConstructResponse(result);
        }

        public WebResponse PostElements(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            List<string> missingParameters = GetMissingParameters(dict, new List<string>() { "using", "value" });
            if (missingParameters.Count > 0)
            {
                return ConstructMissingParameterResponse(missingParameters);
            }

            Dictionary<string, object> result = BrowserState.FindElements((string)dict["using"], (string)dict["value"], null);
            return ConstructResponse(result);
        }

        public WebResponse PostElementActive(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetElement(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostElementElement(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            List<string> missingParameters = GetMissingParameters(dict, new List<string>() { "using", "value" });
            if (missingParameters.Count > 0)
            {
                return ConstructMissingParameterResponse(missingParameters);
            }

            Dictionary<string, object> result = BrowserState.FindElement((string)dict["using"], (string)dict["value"], matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse PostElementElements(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            List<string> missingParameters = GetMissingParameters(dict, new List<string>() { "using", "value" });
            if (missingParameters.Count > 0)
            {
                return ConstructMissingParameterResponse(missingParameters);
            }

            Dictionary<string, object> result = BrowserState.FindElements((string)dict["using"], (string)dict["value"], matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse PostElementClick(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.Click(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse PostElementSubmit(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.Submit(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse GetElementText(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.GetText(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse PostElementValue(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            List<string> missingParameters = GetMissingParameters(dict, new List<string>() { "value" });
            if (missingParameters.Count > 0)
            {
                return ConstructMissingParameterResponse(missingParameters);
            }

            object[] keySequences = dict["value"] as object[];
            if (keySequences == null)
            {
            }

            StringBuilder keys = new StringBuilder();
            foreach (object keySequence in keySequences)
            {
                keys.Append(keySequence.ToString());
            }

            Dictionary<string, object> result = BrowserState.Type(matchedApiValues["id"], keys.ToString());
            return ConstructResponse(result);
        }

        public WebResponse PostKeys(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetElementName(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.GetTagName(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse PostElementClear(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.ElementClear(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse GetElementSelected(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.IsSelected(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse GetElementEnabled(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.IsEnabled(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse GetElementAttribute(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.GetAttribute(matchedApiValues["id"], matchedApiValues["name"]);
            return ConstructResponse(result);
        }

        public WebResponse GetElementEquals(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetElementDisplayed(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.IsDisplayed(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse GetElementLocation(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.GetLocation(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse GetElementLocationInView(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetElementSize(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.GetSize(matchedApiValues["id"]);
            return ConstructResponse(result);
        }

        public WebResponse GetElementCss(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string internalId = matchedApiValues["id"];
            string propertyName = matchedApiValues["propertyName"];

            Dictionary<string, object> result = BrowserState.GetCssProperty(internalId, propertyName);
            return ConstructResponse(result);
        }

        public WebResponse GetOrientation(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostOrientation(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetAlertText(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostAlertText(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostAcceptAlert(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostDismissAlert(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostMoveTo(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostClick(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> result = BrowserState.Click(matchedApiValues["id"]);
            return ConstructResponse(result); ;
        }

        public WebResponse PostButtonDown(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostButtonUp(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostTouchClick(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostTouchDown(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostTouchUp(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostTouchMove(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostTouchScroll(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostDoubleClick(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostLongClick(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostTouchFlick(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetLocation(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostLocation(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetLocalStorage(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostLocalStorage(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse DeleteLocalStorage(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetLocalStorageKey(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse DeleteLocalStorageKey(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetLocalStorageSize(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetSessionStorage(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostSessionStorage(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse DeleteSessionStorage(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetSessionStorageKey(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse DeleteSessionStorageKey(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetSessionStorageSize(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostLog(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostLogTypes(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetApplicationCacheStatus(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        private static WebResponse ConstructResponse(Dictionary<string, object> responseValue)
        {
            WebResponse response = new WebResponse();
            response.ContentType = "application/json;charset=utf-8";
            string serializedValue = WebDriverWireProtocolJsonConverter.Serialize(responseValue);
            if (responseValue.ContainsKey("status"))
            {
                JsonWire.ResponseCode status = (JsonWire.ResponseCode)responseValue["status"];
                if (status == JsonWire.ResponseCode.Sucess)
                {
                    response.StatusCode = 200;
                }
                else
                {
                    response.StatusCode = 500;
                }
            }

            response.Content = serializedValue;
            return response;
        }

        private static WebResponse ConstructMissingParameterResponse(List<string> missingParameters)
        {
            WebResponse response = new WebResponse();
            response.StatusCode = 400;
            response.ContentType = "text/html;charset=utf-8";
            response.Content = string.Join(",", missingParameters.ToArray());
            return response;
        }

        private static List<string> GetMissingParameters(Dictionary<string, object> parameters, List<string> requiredParameters)
        {
            List<string> missingParameters = new List<string>();
            foreach (string parameter in parameters.Keys)
            {
                if (!requiredParameters.Contains(parameter))
                {
                    missingParameters.Add(parameter);
                }
            }

            return missingParameters;
        }
    }
}
