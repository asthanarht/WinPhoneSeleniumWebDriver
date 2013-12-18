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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPhoneDriver
{
    public class MiniHttpServer
    {
        public delegate WebResponse RequestHandler(WebRequest request, Dictionary<string, string> matchedApiValues);

        private Dictionary<ApiPattern, RequestHandler> handlersTable;
        private Listener listener;

        public MiniHttpServer(UInt16 port)
        {
            handlersTable = new Dictionary<ApiPattern, RequestHandler>();
            listener = new Listener(port, OnConnection);
        }

        private async void OnConnection(SocketIO socketIO)
        {
            WebRequest request = await ReadRequest(socketIO);
            Helpers.Log("Operating request {0} {1}", request.Method, request.ResurcePath);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            KeyValuePair<ApiPattern, RequestHandler> handlePair = handlersTable.FirstOrDefault(h => ApiPattern.Match(h.Key, request, out dict) == true);

            if (handlePair.Key == ApiPattern.Empty)
            {
                Helpers.Log("Unimplemented method called");
                await ResponseWith(WebResponse.NotImplemented(), socketIO);
                socketIO.Die();
            }
            else
            {
                bool success = true;
                try
                {
                    await ResponseWith(handlePair.Value(request, dict), socketIO);
                }
                catch
                {
                    //Exception inside handling routine, insead of dying, gracefully
                    //resonse with fixed message
                    //WE CAN NOT AWAIT INSIDE CATCH
                    success = false;
                }

                if (success == false)
                {
                    await ResponseWith(WebResponse.InternalServerError(), socketIO);
                }

                socketIO.Die();
            }
        }

        private async Task ResponseWith(WebResponse response, SocketIO socketIO)
        {
            await socketIO.WriteString(response.ToString());
        }

        public void RegisterHandle(ApiPattern pattern, RequestHandler handle)
        {
            handlersTable.Add(pattern, handle);
        }

        private async Task<WebRequest> ReadRequest(SocketIO socketIO)
        {
            WebRequest result = new WebRequest();
            string line = await socketIO.ReadLine();
            string[] splits = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            result.Method = GetMethod(splits[0]);

            result.ResurcePath = splits[1];
            result.HttpVerison = splits[2];
            result.Headers = await GetHeaders(socketIO);
            result.Content = await socketIO.ReadString(result.ContentLength);

            return result;
        }


        private async Task<Dictionary<WebRequest.WebRequestHeader, string>> GetHeaders(SocketIO socketIO)
        {

            Dictionary<WebRequest.WebRequestHeader, string> result = new Dictionary<WebRequest.WebRequestHeader, string>();
            while (true)
            {
                string line = await socketIO.ReadLine();
                string[] splits = line.Split(new char[] { ':' }, 2);

                if (splits.Length < 2)
                {
                    return result;
                }

                switch (splits[0])
                {
                    case "Content-Length":
                        result[WebRequest.WebRequestHeader.ContentLength] = splits[1].Trim(); break;
                    case "Content-Type":
                        result[WebRequest.WebRequestHeader.ContentType] = splits[1].Trim(); break;
                    case "Cookie":
                        result[WebRequest.WebRequestHeader.Cookie] = splits[1].Trim(); break;
                    case "Accept":
                        result[WebRequest.WebRequestHeader.Accept] = splits[1].Trim(); break;
                }
            }
        }

        private WebRequest.WebRequestMethod GetMethod(string methodText)
        {
            methodText.ToUpper();

            switch (methodText)
            {
                case "GET":
                    return WebRequest.WebRequestMethod.Get;
                case "POST":
                    return WebRequest.WebRequestMethod.Post;
                case "DELETE":
                    return WebRequest.WebRequestMethod.Delete;
                case "PUT":
                    return WebRequest.WebRequestMethod.Put;
            }

            throw new Exception("Unknown method specyfied");
        }

        public void RegisterHandlers(RequestHandlers handlers)
        {
            this.RegisterHandle(
                  new ApiPattern(WebRequest.WebRequestMethod.Get, "/status"),
                  new MiniHttpServer.RequestHandler(handlers.GetStatus)
                  );

            this.RegisterHandle(
                  new ApiPattern(WebRequest.WebRequestMethod.Post, "/session"),
                  new MiniHttpServer.RequestHandler(handlers.PostSession)
                  );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId"),
                new MiniHttpServer.RequestHandler(handlers.GetSession)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Delete, "/session/:sessionId"),
                new MiniHttpServer.RequestHandler(handlers.DeleteSession)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/url"),
                new MiniHttpServer.RequestHandler(handlers.PostUrl)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/forward"),
                new MiniHttpServer.RequestHandler(handlers.PostForward)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/back"),
                new MiniHttpServer.RequestHandler(handlers.PostBack)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/refresh"),
                new MiniHttpServer.RequestHandler(handlers.PostRefresh)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/screenshot"),
                new MiniHttpServer.RequestHandler(handlers.GetScreenshot)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/element"),
                new MiniHttpServer.RequestHandler(handlers.PostElement)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/element/:id/element"),
                new MiniHttpServer.RequestHandler(handlers.PostElementElement)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/element/:id/elements"),
                new MiniHttpServer.RequestHandler(handlers.PostElementElements)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/element/:id/text"),
                new MiniHttpServer.RequestHandler(handlers.GetElementText)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/element/:id/name"),
                new MiniHttpServer.RequestHandler(handlers.GetElementName)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/element/:id/attribute/:name"),
                new MiniHttpServer.RequestHandler(handlers.GetElementAttribute)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/element/:id/click"),
                new MiniHttpServer.RequestHandler(handlers.PostClick)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/element/:id/value"),
                new MiniHttpServer.RequestHandler(handlers.PostElementValue)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/cookie"),
                new MiniHttpServer.RequestHandler(handlers.GetCookie)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/cookie"),
                new MiniHttpServer.RequestHandler(handlers.PostCookie)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Delete, "/session/:sessionId/cookie"),
                new MiniHttpServer.RequestHandler(handlers.DeleteCookie)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/window_handle"),
                new MiniHttpServer.RequestHandler(handlers.GetWindowHandle)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/elements"),
                new MiniHttpServer.RequestHandler(handlers.PostElements)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/element/:id/submit"),
                new MiniHttpServer.RequestHandler(handlers.PostElementSubmit)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/source"),
                new MiniHttpServer.RequestHandler(handlers.GetSource)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/title"),
                new MiniHttpServer.RequestHandler(handlers.GetTitle)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/element/:id/size"),
                new MiniHttpServer.RequestHandler(handlers.GetElementSize)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/element/:id/selected"),
                new MiniHttpServer.RequestHandler(handlers.GetElementSelected)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Delete, "/session/:sessionId/"),
                new MiniHttpServer.RequestHandler(handlers.DeleteSession)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/element/:id/location"),
                new MiniHttpServer.RequestHandler(handlers.GetElementLocation)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Delete, "/session/"),
                new MiniHttpServer.RequestHandler(handlers.DeleteSession)
             );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/element/:id/clear"),
                new MiniHttpServer.RequestHandler(handlers.PostElementClear)
             );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/element/:id/displayed"),
                new MiniHttpServer.RequestHandler(handlers.GetElementDisplayed)
            );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/element/:id/enabled"),
                new MiniHttpServer.RequestHandler(handlers.GetElementEnabled)
            );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Get, "/session/:sessionId/element/:id/css/:propertyName"),
                new MiniHttpServer.RequestHandler(handlers.GetElementCss)
                );
            
            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/execute"),
                new MiniHttpServer.RequestHandler(handlers.PostExecute)
            );
            
            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/frame"),
                new MiniHttpServer.RequestHandler(handlers.PostFrame)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/buttondown"),
                new MiniHttpServer.RequestHandler(handlers.PostButtonDown)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/buttonup"),
                new MiniHttpServer.RequestHandler(handlers.PostButtonUp)
                );

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/moveto"),
                new MiniHttpServer.RequestHandler(handlers.PostButtonUp)
            ); 

            this.RegisterHandle(
                new ApiPattern(WebRequest.WebRequestMethod.Post, "/session/:sessionId/doubleclick"),
                new MiniHttpServer.RequestHandler(handlers.PostDoubleClick)
                );
        }
    }
}
