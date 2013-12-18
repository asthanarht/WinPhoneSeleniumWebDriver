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
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsPhoneDriver
{
    public class AutomatedWebBrowser
    {
        private const string WindowReferenceKey = "WINDOW";
        private const string ElementReferenceKey = "ELEMENT";

        private ManualResetEvent operationLock = new ManualResetEvent(true);

        private Dispatcher dispatcher;
        private MainPage page;
        private Guid sessionId;
        private WebBrowser browser;
        private string focusedFrame;

        public AutomatedWebBrowser(Dispatcher mainThreadDispatcher, MainPage page)
        {
            this.dispatcher = mainThreadDispatcher;
            this.page = page;
            this.browser = page.Browser;
        }

        public void SignalComplete()
        {
            this.operationLock.Set();
        }

        public Dictionary<string, object> DescribeSession()
        {
            Dictionary<string, object> sessionCapabilities = new Dictionary<string, object>();
            sessionCapabilities["browserAttachTimeout"] = 0;
            sessionCapabilities["browserName"] = "internet explorer";
            sessionCapabilities["cssSelectorsEnabled"] = true;
            sessionCapabilities["elementScrollBehavior"] = 0;
            sessionCapabilities["handlesAlerts"] = false;
            sessionCapabilities["javascriptEnabled"] = true;
            sessionCapabilities["nativeEvents"] = false;
            sessionCapabilities["platform"] = "WINDOWS";
            sessionCapabilities["takesScreenshot"] = true;
            sessionCapabilities["unexpectedAlertBehaviour"] = "ignore";
            sessionCapabilities["version"] = "10";

            Dictionary<string, object> responseObject = new Dictionary<string, object>();
            responseObject["sessionId"] = this.sessionId.ToString();
            responseObject["status"] = JsonWire.ResponseCode.Sucess;
            responseObject["value"] = sessionCapabilities;

            return responseObject;
        }

        public Dictionary<string, object> EndSession()
        {
            Dictionary<string, object> responseObject = new Dictionary<string, object>();
            responseObject["sessionId"] = this.sessionId.ToString();
            responseObject["status"] = JsonWire.ResponseCode.Sucess;
            responseObject["value"] = null;

            return responseObject;
        }

        public Dictionary<string, object> GetCurrentWindowHandle()
        {
            Dictionary<string, object> responseObject = new Dictionary<string, object>();
            responseObject["sessionId"] = this.sessionId.ToString();
            responseObject["status"] = JsonWire.ResponseCode.Sucess;
            responseObject["value"] = "FakeHandle";

            return responseObject;
        }

        public Dictionary<string, object> GetAllWindowHandles()
        {
            Dictionary<string, object> responseObject = new Dictionary<string, object>();
            responseObject["sessionId"] = this.sessionId.ToString();
            responseObject["status"] = JsonWire.ResponseCode.Sucess;
            responseObject["value"] = responseObject["value"] = new object[] { "FakeHandle" };

            return responseObject;
        }

        protected void CallInMainThread(Action d)
        {
            this.operationLock.Reset();
            this.dispatcher.BeginInvoke(d);
            this.operationLock.WaitOne();
        }

        public void ClearCacheAndCookies()
        {
            CallInMainThread(
                () => {this.ClearCacheAndCookiesInMainThread();});
        }

        private async void ClearCacheAndCookiesInMainThread()
        {
            await browser.ClearCookiesAsync();
            await browser.ClearInternetCacheAsync();

            this.operationLock.Set();
        }

        public Dictionary<string, object> Navigate(string uri)
        {
            Uri parsedUri;
            if (Uri.TryCreate(uri, UriKind.Absolute, out parsedUri))
            {
                this.AttachBrowserEvents();

                CallInMainThread(() =>
                {
                    this.browser.Navigate(parsedUri);
                });

                this.DetachBrowserEvents();

                this.focusedFrame = string.Empty;
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["sessionId"] = this.sessionId.ToString();
            result["status"] = JsonWire.ResponseCode.Sucess;
            result["value"] = null;
            return result;
        }

        public Dictionary<string, object> GoForward()
        {
            this.AttachBrowserEvents();

            CallInMainThread(() =>
            {
                if (this.browser.CanGoForward)
                {
                    this.browser.GoForward();
                }
            });

            this.DetachBrowserEvents();

            this.focusedFrame = string.Empty;

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["sessionId"] = this.sessionId.ToString();
            result["status"] = JsonWire.ResponseCode.Sucess;
            result["value"] = null;
            return result;
        }

        public Dictionary<string, object> GoBack()
        {
            this.AttachBrowserEvents();

            CallInMainThread(() =>
            {
                if (this.browser.CanGoBack)
                {
                    this.browser.GoBack();
                }
            });

            this.DetachBrowserEvents();

            this.focusedFrame = string.Empty;

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["sessionId"] = this.sessionId.ToString();
            result["status"] = JsonWire.ResponseCode.Sucess;
            result["value"] = null;
            return result;
        }

        public Dictionary<string, object> Refresh()
        {
            this.AttachBrowserEvents();

            CallInMainThread(() =>
            {
                this.browser.Navigate(new Uri(this.browser.Source.AbsoluteUri));
            });

            this.DetachBrowserEvents();

            this.focusedFrame = string.Empty;

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["sessionId"] = this.sessionId.ToString();
            result["status"] = JsonWire.ResponseCode.Sucess;
            result["value"] = null;
            return result;
        }

        public Dictionary<string, object> GetUrl()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["sessionId"] = this.sessionId.ToString();
            result["status"] = JsonWire.ResponseCode.Sucess;
            result["value"] = this.browser.Source.ToString();
            return result;
        }

        public string CreateNewSession()
        {
            this.sessionId = Guid.NewGuid();

            return this.sessionId.ToString();
        }

        public bool CheckSession(string sessionIdstring)
        {
            return sessionIdstring == this.sessionId.ToString();
        }

        public Dictionary<string, object> GetScreenshot()
        {
            string screenshot = string.Empty;
            CallInMainThread(() =>
            {
                screenshot = this.page.GetScreenShot();
                this.operationLock.Set();
            });

            Dictionary<string, object> screenshotResult = new Dictionary<string, object>();
            screenshotResult["status"] = JsonWire.ResponseCode.Sucess;
            screenshotResult["sessionId"] = this.sessionId.ToString();
            screenshotResult["value"] = screenshot;
            return screenshotResult;
        }

        public Dictionary<string, object> FindElement(string strategy, string value, string parentElementId)
        {
            strategy = Helpers.WrapString(strategy);
            value = Helpers.WrapString(value);
            string atomResult = InjectAtomsScript(WebDriverAtoms.FindElement, strategy, value, ConstructElementReference(parentElementId), this.ConstructFrameReference());
            Dictionary<string, object> resultObject = DeserializeAtomResult(atomResult);

            // The atoms stupidly don't return an error for no element found, but
            // rather a null value member. Check for this condition.
            object valueObject = null;
            if (resultObject.TryGetValue("value", out valueObject))
            {
                if (valueObject == null)
                {
                    resultObject["status"] = (int)JsonWire.ResponseCode.NoSuchElement;
                    Dictionary<string, object> errorDetails = new Dictionary<string, object>();
                    errorDetails["message"] = string.Format("No element found with {0} of '{1}'", strategy, value);
                    resultObject["value"] = errorDetails;
                }
            }

            return resultObject;
        }

        public Dictionary<string, object> FindElements(string strategy, string value, string parentElementId)
        {
            strategy = Helpers.WrapString(strategy);
            value = Helpers.WrapString(value);
            string atomResult = InjectAtomsScript(WebDriverAtoms.FindElements, strategy, value, ConstructElementReference(parentElementId), this.ConstructFrameReference());
            Dictionary<string, object> resultObject = DeserializeAtomResult(atomResult);

            // The atoms stupidly don't return an error for no element found, but
            // rather a null value member. Check for this condition.
            object valueObject = null;
            if (resultObject.TryGetValue("value", out valueObject))
            {
                if (valueObject == null)
                {
                    resultObject["status"] = (int)JsonWire.ResponseCode.NoSuchElement;
                    Dictionary<string, object> errorDetails = new Dictionary<string, object>();
                    errorDetails["message"] = string.Format("No element found with {0} of '{1}'", strategy, value);
                    resultObject["value"] = errorDetails;
                }
            }

            return resultObject;
        }

        private Dictionary<string, object> DeserializeAtomResult(string atomResult)
        {
            Dictionary<string, object> deserializedResult = WebDriverWireProtocolJsonConverter.Deserialize(atomResult);
            deserializedResult["sessionId"] = this.sessionId.ToString();
            int status = Convert.ToInt32(deserializedResult["status"]);
            deserializedResult["status"] = (JsonWire.ResponseCode)status;
            return deserializedResult;
        }

        private string InjectAtomsScript(string script, params object[] paramaters)
        {
            string wrappedScript = Helpers.WrapFunctionCallWithResult(script, paramaters);
            object returnValue = EvalJs(wrappedScript);
            return returnValue.ToString();
        }

        public Dictionary<string, object> GetSource()
        {
            string source = string.Empty;
            CallInMainThread(() =>
            {
                source = this.browser.SaveToString();
                this.operationLock.Set();
            });

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["sessionId"] = this.sessionId.ToString();
            result["status"] = JsonWire.ResponseCode.Sucess;
            result["value"] = source;
            return result;
        }

        public Dictionary<string, object> GetTitle()
        {
            string titleScript = "\"if(window && window.top && window.top.document) { return window.top.document.title; } return '';\"";
            string atomResult = InjectAtomsScript(WebDriverAtoms.ExecuteScript, titleScript, "[]");
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> Click(string internalId)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.Click, ConstructElementReference(internalId), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> Type(string internalId, string keys)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.Type, ConstructElementReference(internalId), Helpers.WrapString(keys), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> ElementClear(string internalId)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.Clear, ConstructElementReference(internalId), this.ConstructFrameReference()).ToString();
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> GetCookies()
        {
            CookieCollection cookieJar = null;
            CallInMainThread(() =>
            {
                cookieJar = this.browser.GetCookies();
                this.operationLock.Set();
            });

            List<object> cookieList = new List<object>();
            foreach (Cookie cookie in cookieJar)
            {
                Dictionary<string, object> cookieObject = new Dictionary<string, object>();
                cookieObject["name"] = cookie.Name;
                cookieObject["value"] = cookie.Value;
                cookieObject["path"] = cookie.Path;
                cookieObject["domain"] = cookie.Domain;
                cookieObject["secure"] = cookie.Secure;
                cookieObject["expires"] = cookie.Expires.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                cookieList.Add(cookieObject);
            }

            Dictionary<string, object> result = new Dictionary<string, object>();
            result["sessionId"] = this.sessionId.ToString();
            result["status"] = JsonWire.ResponseCode.Sucess;
            result["value"] = cookieList.ToArray();
            return result;
        }

        public Dictionary<string, object> AddCookie(Dictionary<string, object> cookie)
        {
            StringBuilder cookieBuilder = new StringBuilder();
            cookieBuilder.AppendFormat("{0}={1}; ", cookie["name"], cookie["value"]);

            if (cookie.ContainsKey("secure"))
            {
                bool isSecure = Convert.ToBoolean(cookie["secure"]);
                if (isSecure)
                {
                    cookieBuilder.Append("secure; ");
                }
            }

            if (cookie.ContainsKey("expiry"))
            {
                double expirationOffset = Convert.ToDouble(cookie["expiry"]);
                DateTime expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expirationOffset);
                cookieBuilder.AppendFormat("expires={0:ddd, d MMM yyyy HH:mm:ss} GMT; ", expires);
            }

            if (cookie.ContainsKey("path"))
            {
                cookieBuilder.AppendFormat("path={0}; ", cookie["path"].ToString());
            }

            if (cookie.ContainsKey("domain"))
            {
                cookieBuilder.AppendFormat("domain={0}; ", cookie["domain"].ToString());
            }

            string result = InjectAtomsScript(WebDriverAtoms.ExecuteScript, "document.cookie = '" + cookieBuilder.ToString() + "';");
            return DeserializeAtomResult(result);
        }

        public Dictionary<string, object> Submit(string internalId)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.Submit, internalId, this.ConstructFrameReference()).ToString();
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> GetCssProperty(string internalId, string propertyName)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.GetValueOfCssProperty, ConstructElementReference(internalId), Helpers.WrapString(propertyName), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> IsDisplayed(string internalId)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.IsDisplayed, ConstructElementReference(internalId), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> IsEnabled(string internalId)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.IsEnabled, ConstructElementReference(internalId), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> Execute(string script, string args)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.ExecuteScript, Helpers.WrapString(script), args, this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> GetLocation(string internalId)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.GetTopLeftCoordinates, ConstructElementReference(internalId), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> IsSelected(string internalId)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.IsSelected, ConstructElementReference(internalId), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> GetSize(string internalId)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.GetSize, ConstructElementReference(internalId), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> GetTagName(string internalId)
        {
            string tagNameScript = "\"return arguments[0].tagName;\"";
            string tagNameArgs = string.Format("[{0}]", ConstructElementReference(internalId));
            string atomResult = InjectAtomsScript(WebDriverAtoms.ExecuteScript, tagNameScript, tagNameArgs, this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> GetText(string internalId)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.GetText, ConstructElementReference(internalId), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> GetAttribute(string internalId, string name)
        {
            string atomResult = InjectAtomsScript(WebDriverAtoms.GetAttributeValue, ConstructElementReference(internalId), Helpers.WrapString(name), this.ConstructFrameReference());
            return DeserializeAtomResult(atomResult);
        }

        public Dictionary<string, object> SwitchToFrame(string frameIdentifier)
        {
            Dictionary<string, object> resultObject = new Dictionary<string, object>();
            string result = string.Empty;
            if (frameIdentifier == null)
            {
                InjectAtomsScript(WebDriverAtoms.DefaultContent);
                this.focusedFrame = string.Empty;
                resultObject["status"] = JsonWire.ResponseCode.Sucess;
                resultObject["value"] = null;
            }
            else
            {
                string frameId = frameIdentifier;
                string frameAtom = WebDriverAtoms.FrameByIdOrName;
                if (frameIdentifier.Contains(ElementReferenceKey))
                {
                    frameAtom = WebDriverAtoms.GetFrameWindow;
                }
                else
                {
                    int frameIndex = 0;
                    if (int.TryParse(frameIdentifier, out frameIndex))
                    {
                        frameAtom = WebDriverAtoms.FrameByIndex;
                    }
                    else
                    {
                        frameIdentifier = Helpers.WrapString(frameIdentifier);
                    }
                }

                result = InjectAtomsScript(frameAtom, frameIdentifier, this.ConstructFrameReference());
                resultObject = DeserializeAtomResult(result);
                object valueObject = null;
                if (resultObject.TryGetValue("value", out valueObject))
                {
                    if (valueObject == null)
                    {
                        resultObject["status"] = (int)JsonWire.ResponseCode.NoSuchFrame;
                        Dictionary<string, object> errorDetails = new Dictionary<string, object>();
                        errorDetails["message"] = string.Format("No frame found with identifier '{0}'", frameIdentifier);
                        resultObject["value"] = errorDetails;
                    }
                    else
                    {
                        Dictionary<string, object> valueAsDictionary = valueObject as Dictionary<string, object>;
                        object windowId = null;
                        if (valueAsDictionary != null && valueAsDictionary.TryGetValue(WindowReferenceKey, out windowId))
                        {
                            this.focusedFrame = windowId.ToString();
                        }
                    }
                }
            }

            return resultObject;
        }

        /// <summary>
        /// Creates a serializable object for the currently focused frame.
        /// </summary>
        /// <returns>A JSON-serialized reference to the currently focused frame.
        /// Returns <see langword="null"/> if focused on the top-level frame.</returns>
        public string ConstructFrameReference()
        {
            if (string.IsNullOrEmpty(this.focusedFrame))
            {
                return null;
            }

            Dictionary<string, object> frameObject = new Dictionary<string, object>();
            frameObject[WindowReferenceKey] = this.focusedFrame;
            string frameReference = JsonConvert.SerializeObject(frameObject);
            return frameReference;
        }

        private string ConstructElementReference(string elementId)
        {
            if (elementId == null)
            {
                return null;
            }

            Dictionary<string, object> elementObject = new Dictionary<string, object>();
            elementObject[ElementReferenceKey] = elementId;
            string elementReference = JsonConvert.SerializeObject(elementObject);
            return elementReference;
        }

        /// <summary>
        /// Function will evaluete given JS script inside webBrowser.
        /// The problem is, this function will return last expression in script
        /// that evalutes to string, otherwise it's null.
        /// </summary>
        /// <param name="jsScript">Js script content.</param>
        /// <returns>Null object or last evaluted string.</returns>
        private object EvalJs(string jsScript)
        {
            //Doulbe check if page is loaded
            //Method 'InvokeScript' tends to hangs randomlny
            //For sake of this I just feed it with js 5 times if any exception is thrown
            object returnValue = null;
            CallInMainThread(() =>
            {
                page.EvaluateJavaScript(jsScript, out returnValue);
            });

            if (returnValue is Exception)
            {
                throw (Exception)returnValue;
            }

            if (returnValue != null)
            {
                Helpers.Log("Script returned: {0}", returnValue.ToString());
            }

            return returnValue;
        }

        private void AttachBrowserEvents()
        {
            this.browser.Navigated += browser_Navigated;
            this.browser.NavigationFailed += browser_NavigationFailed;
            this.browser.LoadCompleted += browser_LoadCompleted;
        }

        private void DetachBrowserEvents()
        {
            this.browser.LoadCompleted -= browser_LoadCompleted;
            this.browser.NavigationFailed -= browser_NavigationFailed;
            this.browser.Navigated -= browser_Navigated;
        }

        private void browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Helpers.Log("Load completed");
            this.operationLock.Set();
        }

        private void browser_NavigationFailed(object sender, System.Windows.Navigation.NavigationFailedEventArgs e)
        {
            Helpers.Log("Navigation failed");
            this.operationLock.Set();
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Helpers.Log("Navigation completed");
        }
    }
}
