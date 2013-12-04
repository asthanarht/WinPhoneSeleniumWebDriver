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

using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.ApplicationModel;
using Windows.Networking.Connectivity;
using Windows.System;
using System.Net.Sockets;
using System.Threading;

namespace WindowsPhoneDriver
{
    public partial class MainPage : PhoneApplicationPage
    {
        public AutomatedWebBrowser BrowserState;
        public Config Configuration;

        public MainPage()
        {
            InitializeComponent();
        }

        public void ShowIPaddresses()
        {
            var hostNames = NetworkInformation.GetHostNames();
            Text.Text = string.Empty;

            foreach (var hn in hostNames)
            {
                if (hn.IPInformation != null)
                {
                    Text.Text += hn.DisplayName + "\n";

                    Helpers.Log("Ip -> {0}", hn.DisplayName);
                }
            }
        }

        private string GetIPXml()
        {
            var hostNames = NetworkInformation.GetHostNames();
            string result = "<List>";

            foreach (var hn in hostNames)
            {
                if (hn.IPInformation != null)
                {
                    result += "<Ip>" + hn.DisplayName + "</Ip>";
                }
            }

            return result + "</List>";
        }
        
        public void EvaluateJavaScript(string script, out object returnValue)
        {
            //TODO: configure via XML
            int count = 10;

            while (true)
            {
                try
                {
                    Browser.InvokeScript("execScript", script);
                    returnValue = Browser.InvokeScript("eval", "window.top.document.__wd_fn_result").ToString();
                    break;
                }
                catch 
                {
                    Helpers.Log("InvokeScript exception");
                    count--;
                    if (count == 0)
                    {
                        //TODO: respond with NoSuchWindow or other error instead of dying
                        returnValue = new Exception();
                        break;
                    }
                    Helpers.Log("Retrying in 500ms");
                    Thread.Sleep(500);
                }
            }

            Helpers.Log("InvokeScript succeeded");
            BrowserState.OperationLock.Set();
        }

        public void Configurate()
        {
            using (IsolatedStorageFile f = IsolatedStorageFile.GetUserStoreForApplication())
            {
                /*Provide configuration for host machine*/
                try
                {
                    IsolatedStorageFileStream newFile = f.OpenFile("WindowsPhoneConfig.xml", FileMode.OpenOrCreate);
                    using (var stream = new StreamWriter(newFile))
                    {
                        stream.Write(GetIPXml());
                    }
                }
                catch
                {
                    throw;
                }

                while (f.FileExists("WebDriverConfig.xml") == false)
                {
                    System.Threading.Thread.Sleep(100);
                }
                try
                {
                    using (var configFile = f.OpenFile("WebDriverConfig.xml", FileMode.Open))
                    {
                        this.Configuration = new Config(configFile);
                    }
                }
                catch
                {
                    //TODO
                    throw;
                }
                finally
                {
                    f.DeleteFile("WebDriverConfig.xml");
                }
            }
        }

        public void GetScreenShot()
        {
            int x = (int)Browser.ActualWidth;
            int y = (int)Browser.ActualHeight;

            var bmp = new WriteableBitmap(x, y);

            bmp.Render(Browser, null);
            bmp.Invalidate();

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.SaveJpeg(ms, x, y, 0, 100);
                byte[] imageBytes = ms.ToArray();

                string base64String = Convert.ToBase64String(imageBytes);

                BrowserState.ScreenShot = base64String;
            }
            BrowserState.OperationLock.Set();
        }

        private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Helpers.Log("Load completed");
            //Browser.InvokeScript("eval", BrowserState.GetJs("Functions.js"));
            //Make sure that scripts are enabled inside the browser
            //It sometimes changes, dont't know why
            Browser.IsScriptEnabled = true;

            //unblock thread
            BrowserState.OperationLock.Set();
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
#if AUTOSTART == false
            Configurate();
#endif
            BrowserState = new AutomatedWebBrowser(Dispatcher, this);
#if AUTOSTART == false
            MiniHttpServer server = new MiniHttpServer(Configuration.Port);
#else
            MiniHttpServer server = new MiniHttpServer(8080);
#endif
            RequestHandlers handlers = new RequestHandlers(BrowserState);
            server.RegisterHandlers(handlers);
        }
    }
}