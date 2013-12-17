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
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace WindowsPhoneDriver
{
    class SocketIO
    {
        private DataReader reader;
        private DataWriter writer;
        private StreamSocket socket;

        public SocketIO(StreamSocket sock)
            : this(sock.InputStream, sock.OutputStream)
        {
            this.socket = sock;
        }

        public void Die()
        {
            socket.Dispose();
        }

        public SocketIO(IInputStream inputStream, IOutputStream outputStream)
        {
            writer = new Windows.Storage.Streams.DataWriter(outputStream);
            reader = new Windows.Storage.Streams.DataReader(inputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;
        }

        public async Task<byte> ReadByte()
        {
            await reader.LoadAsync(1);
            return reader.ReadByte();
        }

        public async Task<string> ReadString(uint size)
        {
            if (size == 0)
            {
                return string.Empty;
            }

            await reader.LoadAsync(size);
            return reader.ReadString(size);
        }

        public async Task<char> ReadChar()
        {
            if (reader.UnconsumedBufferLength == 0)
            {
                await reader.LoadAsync(1);
            }
            

            while (reader.UnconsumedBufferLength == 0);

            string data = reader.ReadString(1);
            return data.First<char>();
        }

        public async Task<string> ReadLine()
        {
            StringBuilder builder = new StringBuilder();
            char a;
            char b;

            while (true)
            {
                a = await ReadChar();

                if (a == '\n')
                {
                    break;
                }

                if (a == '\r')
                {
                    b = await ReadChar();

                    if (b == '\n')
                    {
                        break;
                    }

                    builder.Append(a);
                    builder.Append(b);
                }
                builder.Append(a);
            }

            return builder.ToString();
        }

        public async Task<uint> WriteByte(byte b)
        {
            writer.WriteByte(b);
            return await writer.StoreAsync();
        }

        public async Task<uint> WriteBytes(byte[] bytes)
        {
            writer.WriteBytes(bytes);
            return await writer.StoreAsync();
        }

        public async Task<uint> WriteString(string text)
        {
            writer.WriteString(text);
            return await writer.StoreAsync();
        }

    }
}
