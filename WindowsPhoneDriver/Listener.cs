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
using System.Threading;
using Windows.Networking.Sockets;

namespace WindowsPhoneDriver
{
    delegate void SocketIOHandler(SocketIO socketIO);

    class Listener
    {

        private SocketIOHandler userHandler;
        private Thread detachThread;
        private UInt16 port;

        public Listener(UInt16 port, SocketIOHandler handler)
        {
            this.userHandler = handler;
            this.port = port;

            this.detachThread = new Thread(new ThreadStart(Detach));
            this.detachThread.Start();
        }

        private void ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Helpers.Log("ConnectionReceived");
            SocketIO socketIO = new SocketIO(args.Socket);
            userHandler(socketIO);
        }

        public async void Detach()
        {
            StreamSocketListener sock = new Windows.Networking.Sockets.StreamSocketListener();
            StreamSocketListenerControl control = sock.Control;
            control.QualityOfService = SocketQualityOfService.LowLatency;
            sock.ConnectionReceived += ConnectionReceived;

            await sock.BindServiceNameAsync(port.ToString());

            //Fix this floating thread
            while (true)
            {
                    Thread.Sleep(10000);
            }
        }
    }
}
