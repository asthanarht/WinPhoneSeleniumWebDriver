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

using Newtonsoft.Json;
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
            Dictionary<string, Dictionary<string, string>> responseJson = new Dictionary<string, Dictionary<string, string>>();

            WebResponse response = new WebResponse();

            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";
            //TODO generate this line
            response.Content = "{\"build\": {\"version\":\"0.01\"}, \"os\":{\"version\":\"8\", \"name\": \"Windows Phone 8\"}}";

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
            string sessionId = BrowserState.GetSessionId();

            string content = "{\"sessionId\":\"" + sessionId + "\",\"status\":0,\"value\":{\"browserAttachTimeout\":0,\"browserName\":\"internet explorer\",\"cssSelectorsEnabled\":true,\"elementScrollBehavior\":0,\"enableElementCacheCleanup\":true,\"enablePersistentHover\":true,\"handlesAlerts\":true,\"ignoreProtectedModeSettings\":false,\"ignoreZoomSetting\":false,\"initialBrowserUrl\":\"\",\"javascriptEnabled\":true,\"nativeEvents\":true,\"platform\":\"WINDOWS\",\"requireWindowFocus\":true,\"takesScreenshot\":true,\"unexpectedAlertBehaviour\":\"ignore\",\"version\":\"10\"}}";
            return Helpers.GetSuccessWithContent(sessionId, content);
        }

        public WebResponse DeleteSession(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string content = "{\"sessionId\":\"" + sessionId + "\",\"status\":0,\"value\":null}";
            return Helpers.GetSuccessWithContent(sessionId, content);
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
            string sessionId = BrowserState.GetSessionId();

            return Helpers.GetSuccessWithContent(sessionId, "\"FakeHandle\"");
        }

        public WebResponse GetWindowHandles(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetUrl(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();

            WebResponse response = Helpers.GetSuccessWithContent(
                sessionId,
                Helpers.ToJsonString(BrowserState.CurrentUri)
                );

            return response;
        }

        public WebResponse PostUrl(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            WebResponse response = new WebResponse();

            Dictionary<string, object> dict = request.DeserializeContent();

            if (dict.ContainsKey("url") == true)
            {
                string sessionIdString = BrowserState.GetSessionId();

                response.StatusCode = 200;
                response.ContentType = "application/json;charset=utf-8";
                response.Content = "{\"sessionId\":\"" + sessionIdString + "\",\"status\":0,\"value\":\"\"}";

                this.BrowserState.Navigate((string)dict["url"]);
            }
            else
            {
                //TODO, response with missing parameter
            }

            return response;
        }

        public WebResponse PostForward(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostBack(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostRefresh(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostExecute(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            var dict = request.DeserializeContent();

            //only first overload of Seleniums Execute is implemneted (i.e. you cannot pass parameters)
            string script = dict["script"].ToString();
            string argsString = JsonConvert.SerializeObject(dict["args"]);
            string sessionId = BrowserState.GetSessionId();

            string ret = BrowserState.Execute(script, argsString);

            return Helpers.GetSuccessWithContent(sessionId, ret);
        }

        public WebResponse PostExecuteAsync(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetScreenshot(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string base64Screenshot = BrowserState.GetScreenshot();
            WebResponse response = new WebResponse();
            StringBuilder sb = new StringBuilder();
            string sessionIdString = BrowserState.GetSessionId();

            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";
            response.Content = "{\"sessionId\":\"" + sessionIdString + "\",\"status\":0,\"value\":\"" + base64Screenshot + "\"}"; 

            return response;
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
            string id = null;
            if (dict.ContainsKey("id") && dict["id"] != null)
            {
                id = dict["id"].ToString();
            }

            string result = BrowserState.SwitchToFrame(id);
            WebResponse response = Helpers.GetSuccessWithContent("", result);
            return response;
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
            WebResponse response = new WebResponse();
            CookieCollection cookieJar = this.BrowserState.GetCookies();
            string sessionId = BrowserState.GetSessionId();

            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            string jsonCookieJar = Helpers.BuildJsonCookieJar(cookieJar);

            response.Content = JsonWire.BuildRespose(jsonCookieJar, JsonWire.ResponseCode.Sucess, sessionId);

            return response;
        }

        public WebResponse PostCookie(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
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
            //TODO fix if no url is loaded
            string sessionId = BrowserState.GetSessionId();
            string source = BrowserState.GetSource();
            WebResponse resp = Helpers.GetSuccessWithContent(sessionId, source);

            return resp;
        }

        public WebResponse GetTitle(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse PostElement(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            string elementJson = BrowserState.FindElement((string)dict["using"], (string)dict["value"], null);
            string sessionId = BrowserState.GetSessionId();

            WebResponse response = Helpers.GetSuccessWithContent(sessionId, elementJson);

            return response;
        }

        public WebResponse PostElements(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            string elementsArray = BrowserState.FindElements((string)dict["using"], (string)dict["value"], null);
            string sessionId = BrowserState.GetSessionId();

            WebResponse response = Helpers.GetSuccessWithContent(sessionId, elementsArray);

            return response;
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
            string elementJson = BrowserState.FindElement((string)dict["using"], (string)dict["value"], matchedApiValues["id"]);
            string sessionId = BrowserState.GetSessionId();

            WebResponse response = Helpers.GetSuccessWithContent(sessionId, elementJson);

            return response;
        }

        public WebResponse PostElementElements(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            Dictionary<string, object> dict = request.DeserializeContent();
            string elementsArray = BrowserState.FindElements((string)dict["using"], (string)dict["value"], null);
            string sessionId = BrowserState.GetSessionId();

            WebResponse response = Helpers.GetSuccessWithContent(sessionId, elementsArray);

            return response;
        }

        public WebResponse PostElementClick(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string result = BrowserState.Click(matchedApiValues["id"]);

            WebResponse resp = Helpers.GetSuccessWithContent(sessionId, result);
            return resp;
        }

        public WebResponse PostElementSubmit(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string result = BrowserState.Submit(matchedApiValues["id"]);

            WebResponse resp = Helpers.GetSuccessWithContent(sessionId, result);
            return resp;
        }

        public WebResponse GetElementText(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string text = BrowserState.GetText(matchedApiValues["id"]);

            WebResponse resp = Helpers.GetSuccessWithContent(sessionId, text);

            return resp;
        }

        public WebResponse PostElementValue(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            
            Dictionary<string, char[]> dict = JsonConvert.DeserializeObject<Dictionary<string, char[]>>(request.Content);
            char[] keys = dict["value"];

            string result = BrowserState.Type(matchedApiValues["id"], new string(keys));

            WebResponse resp = Helpers.GetSuccessWithContent(sessionId, result);
            return resp;
        }

        public WebResponse PostKeys(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetElementName(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string tagName = BrowserState.GetTagName(matchedApiValues["id"]);

            return Helpers.GetSuccessWithContent(sessionId, tagName);
        }

        public WebResponse PostElementClear(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            BrowserState.ElementClear(matchedApiValues["id"]);

            return Helpers.GetSuccessWithContent(sessionId, "null");
        }

        public WebResponse GetElementSelected(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string isSelected = BrowserState.IsSelected(matchedApiValues["id"]);

            return Helpers.GetSuccessWithContent(sessionId, isSelected);
        }

        public WebResponse GetElementEnabled(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string isEnabled = BrowserState.IsEnabled(matchedApiValues["id"]);

            return Helpers.GetSuccessWithContent(sessionId, isEnabled);
        }

        public WebResponse GetElementAttribute(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string attrib = BrowserState.GetAttribute(matchedApiValues["id"], matchedApiValues["name"]);

            string sessionId = BrowserState.GetSessionId();

            WebResponse resp = Helpers.GetSuccessWithContent(sessionId, attrib);

            return resp;
        }

        public WebResponse GetElementEquals(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetElementDisplayed(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string isDisplayed = BrowserState.IsDisplayed(matchedApiValues["id"]);

            return Helpers.GetSuccessWithContent(sessionId, isDisplayed);
        }

        public WebResponse GetElementLocation(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string location = BrowserState.GetLocation(matchedApiValues["id"]);

            return Helpers.GetSuccessWithContent(sessionId, location);
        }

        public WebResponse GetElementLocationInView(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            return WebResponse.NotImplemented();
        }

        public WebResponse GetElementSize(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string size = BrowserState.GetSize(matchedApiValues["id"]);

            return Helpers.GetSuccessWithContent(sessionId, size);
        }

        public WebResponse GetElementCss(WebRequest request, Dictionary<string, string> matchedApiValues)
        {
            string sessionId = BrowserState.GetSessionId();
            string internalId = matchedApiValues["id"];
            string propertyName = matchedApiValues["propertyName"];

            string propertyValue = BrowserState.GetCssProperty(internalId, propertyName);

            return Helpers.GetSuccessWithContent(sessionId, propertyValue);
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
            //TODO
            // -response with ElementNotVisible, StaleElementReference, NoShuchWindow
            WebResponse response = new WebResponse();
            string sessionId = BrowserState.GetSessionId();

            string content = BrowserState.Click(matchedApiValues["id"]);

            response.StatusCode = 200;
            response.Content = content;
            response.ContentType = "application/json;charset=utf-8";

            return response;
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
    }

}
