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
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace WindowsPhoneDriver
{
    public class AutomatedWebBrowser
    {
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
            EvalJs(GetJs("Functions.js"));
        }

        public void Navigate(string uri)
        {
            CallInMainThread(
                () => { this.browser.Navigate(new Uri(uri)); });

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

        public string FindElement(string strategy, string value)
        {
            strategy = Helpers.WrapString(strategy);
            value = Helpers.WrapString(value);

            InjectAtomsScript("find_element_ie.js", strategy, value);
            return EvalJs("SeleniumWindowsPhoneDriver.SaveElement(SeleniumWindowsPhoneDriver.LastCallResult);").ToString();
        }

        public string FindElements(string strategy, string value)
        {
            strategy = Helpers.WrapString(strategy);
            value = Helpers.WrapString(value);

            InjectAtomsScript("find_elements_ie.js", strategy, value);
            return EvalJs("SeleniumWindowsPhoneDriver.ConvertElementsToInternlIds(SeleniumWindowsPhoneDriver.LastCallResult);").ToString();
        }

        private string InjectAtomsScript(string path, params object[] paramaters)
        {
            string script = Helpers.WrapFunctionCallWithResult(GetJs(Path.Combine("Atoms", path)), paramaters);
            return EvalJs(script).ToString();
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

        public bool Click(string internalId)
        {
            InjectAtomsScript("click_ie.js", GetElementByInternalIdScript(internalId));

            return string.IsNullOrEmpty("ss");
        }


        public bool Type(string internalId, string keys, int keyboardMode, bool opt_persistModifiers)
        {
            EvalJs(GetJs(@"Atoms\inputs.js"));

            internalId = Helpers.WrapString(internalId);
            keys = Helpers.WrapString(keys);
            string opt_persistModifiersString = "true";

            if (opt_persistModifiers == false)
            {
                opt_persistModifiersString = "false";
            }

            EvalJs(Helpers.WrapFunctionCall("SeleniumWindowsPhoneDriver.Type", internalId, keys, keyboardMode, opt_persistModifiersString));
            return true;
        }

        public string ElementClear(string internalId)
        {
            return InjectAtomsScript("clear_ie.js", GetElementByInternalIdScript(internalId)).ToString();
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

        public void Submit(string internalId)
        {
            InjectAtomsScript("submit_ie.js", GetElementByInternalIdScript(internalId));
        }

        public string GetCssProperty(string internalId, string propertyName)
        {
            InjectAtomsScript("get_effective_style_ie.js", GetElementByInternalIdScript(internalId), Helpers.WrapString(propertyName));

            return ResultToJson();
        }

        public string IsDisplayed(string internalId)
        {
            InjectAtomsScript("is_displayed_ie.js", GetElementByInternalIdScript(internalId));

            return ResultToJson();
        }

        public string IsEnabled(string internalId)
        {
            InjectAtomsScript("is_enabled_ie.js", GetElementByInternalIdScript(internalId));

            return ResultToJson();
        }

        public string GetElementByInternalIdScript(string internalId)
        {
            internalId = Helpers.WrapString(internalId);
            return string.Format("SeleniumWindowsPhoneDriver.GetElement({0})", internalId);
        }

        public string Execute(string script)
        {
            return EvalJs(script).ToString();
        }

        public string GetLocation(string internalId)
        {
            InjectAtomsScript("get_location_ie.js", GetElementByInternalIdScript(internalId));

            return ResultToJson();
        }

        public string IsSelected(string internalId)
        {
            InjectAtomsScript("is_selected_ie.js", GetElementByInternalIdScript(internalId));

            return ResultToJson();
        }

        private string ResultToJson()
        {
            return EvalJs("JSON.stringify(SeleniumWindowsPhoneDriver.LastCallResult);").ToString();
        }

        public string GetSize(string internalId)
        {
            InjectAtomsScript("get_size_ie.js", GetElementByInternalIdScript(internalId));

            return ResultToJson();
        }

        public string GetText(string internalId)
        {
            InjectAtomsScript("get_text_ie.js", GetElementByInternalIdScript(internalId)).ToString();

            return ResultToJson();
        }

        public string GetAttribute(string internalId, string name)
        {
            InjectAtomsScript("get_attribute_ie.js", GetElementByInternalIdScript(internalId), Helpers.WrapString(name));

            return ResultToJson();
        }

        /// <summary>
        /// Function will load file from Js/ dir. It should throw some exception.
        /// </summary>
        /// <param name="scriptPath">path to the script</param>
        /// <returns>Content of script file.</returns>
        public string GetJs(string scriptPath)
        {
            using (FileStream fileStream = File.OpenRead(Path.Combine("Js", scriptPath)))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    try
                    {
                        return reader.ReadToEnd();
                    }
                    catch (IOException exception)
                    {
                        //TODO: inform about this fact on screen
                        Helpers.Log("Problems while opening {0} fatal error: {1}", scriptPath, exception.ToString());
                        App.Current.Terminate();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Very simple templating, function will replace all $\d parameters in js file
        /// with corresponding parameter.
        /// </summary>
        /// <param name="jsScript">Script content</param>
        /// <param name="values">Parametrs values that should substitute $\d vairables</param>
        /// <returns>Processed content of js file (might be unchanged).</returns>
        static private string ProcessJs(string jsScript, params object[] values)
        {
            int i = 0;

            foreach (object param in values)
            {
                Helpers.Log("Param {0}", param.ToString());
                jsScript = jsScript.Replace("$" + i, param.ToString());
                i++;
            }

            return jsScript;
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
