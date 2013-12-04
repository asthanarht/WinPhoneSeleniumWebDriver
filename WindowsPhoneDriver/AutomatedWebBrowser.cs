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

        public string ScreenShot {get; set;}
        public string JsonElementContainer {get; private set;}
        public string SourceContainer {get; private set;}
        public bool LastOperationResult {get; private set;}
        public string CurrentUri {get; private set;}
        public CookieCollection CookieJarContainer {get; private set;}
        public ManualResetEvent OperationLock = new ManualResetEvent(true);

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
            CurrentUri = string.Empty;
        }

        protected void CallInMainThread(Action d)
        {
            OperationLock.Reset();
            this.dispatcher.BeginInvoke(d);
            OperationLock.WaitOne();
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

            this.OperationLock.Set();
        }

        public void InitSelenium()
        {
        }

        public void Navigate(string uri)
        {
            CallInMainThread(
                () => { this.browser.Navigate(new Uri(uri)); });

            this.focusedFrame = string.Empty;
            CurrentUri = uri;
        }

        public string GetUrl()
        {
            if (CurrentUri == null)
            {
                return string.Empty;
            }

            return CurrentUri.ToString();
        }

        public string CreateNewSession()
        {
            this.sessionId = Guid.NewGuid();

            return this.sessionId.ToString();
        }

        public bool CheckSession(string sessionIdstring)
        {
            string guidString = this.sessionId.ToString();

            return sessionIdstring == guidString;
        }

        public string GetScreenshot()
        {
            CallInMainThread(
                () => { this.page.GetScreenShot(); });

            return this.ScreenShot;
        }

        public string FindElement(string strategy, string value, string parentElementId)
        {
            strategy = Helpers.WrapString(strategy);
            value = Helpers.WrapString(value);
            return InjectAtomsScript(WebDriverAtoms.FindElement, strategy, value, ConstructElementReference(parentElementId), this.ConstructFrameReference());
        }

        public string FindElements(string strategy, string value, string parentElementId)
        {
            strategy = Helpers.WrapString(strategy);
            value = Helpers.WrapString(value);
            return InjectAtomsScript(WebDriverAtoms.FindElements, strategy, value, ConstructElementReference(parentElementId), this.ConstructFrameReference());
        }

        private string InjectAtomsScript(string script, params object[] paramaters)
        {
            string wrappedScript = Helpers.WrapFunctionCallWithResult(script, paramaters);
            return EvalJs(wrappedScript).ToString();
        }

        public string GetSessionId()
        {
            return this.sessionId.ToString();
        }

        public string GetSource()
        {
            CallInMainThread(
                () => { this.GetSourceInMainThread(); });

            return this.SourceContainer;
        }

        private void GetSourceInMainThread()
        {
            this.SourceContainer = Helpers.ToJsonString(browser.SaveToString());

            this.OperationLock.Set();
        }

        public string Click(string internalId)
        {
            return InjectAtomsScript(WebDriverAtoms.Click, ConstructElementReference(internalId), this.ConstructFrameReference());
        }

        public string Type(string internalId, string keys)
        {
            return InjectAtomsScript(WebDriverAtoms.Type, ConstructElementReference(internalId), keys, this.ConstructFrameReference());
        }

        public string ElementClear(string internalId)
        {
            return InjectAtomsScript(WebDriverAtoms.Clear, ConstructElementReference(internalId), this.ConstructFrameReference()).ToString();
        }

        public CookieCollection GetCookies()
        {
            CallInMainThread(
                () => { this.GetCookiesInMainThread(); });

            return this.CookieJarContainer;
        }

        private void GetCookiesInMainThread()
        {
            this.CookieJarContainer = this.browser.GetCookies();
            this.OperationLock.Set();
        }

        public string Submit(string internalId)
        {
            return InjectAtomsScript(WebDriverAtoms.Submit, internalId, this.ConstructFrameReference()).ToString();
        }

        public string GetCssProperty(string internalId, string propertyName)
        {
            return InjectAtomsScript(WebDriverAtoms.GetValueOfCssProperty, ConstructElementReference(internalId), Helpers.WrapString(propertyName), this.ConstructFrameReference());
        }

        public string IsDisplayed(string internalId)
        {
            return InjectAtomsScript(WebDriverAtoms.IsDisplayed, ConstructElementReference(internalId), this.ConstructFrameReference());
        }

        public string IsEnabled(string internalId)
        {
            return InjectAtomsScript(WebDriverAtoms.IsEnabled, ConstructElementReference(internalId), this.ConstructFrameReference());
        }

        public string Execute(string script, string args)
        {
            string result = InjectAtomsScript(WebDriverAtoms.ExecuteScript, Helpers.WrapString(script), args, this.ConstructFrameReference());
            return result;
        }

        public string GetLocation(string internalId)
        {
            return InjectAtomsScript(WebDriverAtoms.GetTopLeftCoordinates, ConstructElementReference(internalId), this.ConstructFrameReference());
        }

        public string IsSelected(string internalId)
        {
            return InjectAtomsScript(WebDriverAtoms.IsSelected, ConstructElementReference(internalId), this.ConstructFrameReference());
        }

        public string GetSize(string internalId)
        {
            return InjectAtomsScript(WebDriverAtoms.GetSize, ConstructElementReference(internalId), this.ConstructFrameReference());
        }

        public string GetTagName(string internalId)
        {
            string tagNameScript = "\"return arguments[0].tagName;\"";
            string tagNameArgs = string.Format("[{0}]", ConstructElementReference(internalId));
            return InjectAtomsScript(WebDriverAtoms.ExecuteScript, tagNameScript, tagNameArgs, this.ConstructFrameReference());
        }

        public string GetText(string internalId)
        {
            return InjectAtomsScript(WebDriverAtoms.GetText, ConstructElementReference(internalId), this.ConstructFrameReference());
        }

        public string GetAttribute(string internalId, string name)
        {
            return InjectAtomsScript(WebDriverAtoms.GetAttributeValue, ConstructElementReference(internalId), Helpers.WrapString(name), this.ConstructFrameReference());
        }

        public string SwitchToFrame(string frameIdentifier)
        {
            string result = string.Empty;
            if (frameIdentifier == null)
            {
                InjectAtomsScript(WebDriverAtoms.DefaultContent);
                this.focusedFrame = string.Empty;
                string responseValue = string.Format("{{ \"{0}\" : null }}", WindowReferenceKey);
                return JsonWire.BuildRespose(responseValue, JsonWire.ResponseCode.Sucess, string.Empty);
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
                Dictionary<string, object> resultObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                object valueObject = null;
                if (resultObject.TryGetValue("value", out valueObject))
                {
                    if (valueObject == null)
                    {
                        resultObject["status"] = (int)JsonWire.ResponseCode.NoSuchFrame;
                        Dictionary<string, object> errorDetails = new Dictionary<string, object>();
                        errorDetails["message"] = "No frame found";
                        resultObject["value"] = errorDetails;
                        result = JsonConvert.SerializeObject(resultObject);
                    }
                    else
                    {
                        Type t = valueObject.GetType();
                        JObject valueAsDictionary = valueObject as JObject;
                        JToken token = null;
                        if (valueAsDictionary != null && valueAsDictionary.TryGetValue(WindowReferenceKey, out token))
                        {
                            this.focusedFrame = valueAsDictionary[WindowReferenceKey].ToString();
                        }
                    }
                }
            }

            return result;
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

            return string.Format("{{ \"{0}\": \"{1}\" }}", ElementReferenceKey, elementId);
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
            object returnValue = new object();

            CallInMainThread(
                () =>
                    page.EvaluateJavaScript(jsScript, out returnValue)
                );

            OperationLock.WaitOne();

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
    }
}
