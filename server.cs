using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace loginForm
{
    class Server
    {
        public static bool Start(int port, string proxyto_ip, ushort proxyto_port)
        {
            try
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Loopback, port);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Blocking = true;
                listener.Bind(localEndPoint);
                listener.Listen(100);

                Console.WriteLine("Starting login server for port " + port);
                StartAccept(listener, proxyto_ip, proxyto_port);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        static void StartAccept(Socket listener, string proxyto_ip, ushort proxyto_port)
        {
            listener.BeginAccept(new AsyncCallback(AcceptCallback), new object[] { listener, proxyto_ip, proxyto_port });
        }

        static void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request
            object[] state = (object[])ar.AsyncState;
            Socket listener = (Socket)state[0];
            string proxyto_ip = (string)state[1];
            ushort proxyto_port = (ushort)state[2];

            Socket handler = listener.EndAccept(ar);

            ParameterizedThreadStart start = new ParameterizedThreadStart(LazyThread);
            new Thread(start).Start(new object[] { handler, proxyto_ip, proxyto_port });
        }

        static void LazyThread(object a_objectarr)
        {
            var buf = new Byte[2048];
            int recved;

            object[] state = (object[])a_objectarr;
            Socket handler = (Socket)state[0];
            string proxyto_ip = (string)state[1];
            ushort proxyto_port = (ushort)state[2];

            Console.WriteLine("got lineage client");

            //12 00 7D FA BD 98 7C 41 5A 9B 01 B6 81 01 09 BD CC C0
            //0E 00 C4 9E FB F5 DC E7 85 2C 29 11 16 15
            //1E 00 DA FB D6 92 DF E5 87 44 67 63 7C 3B E7 DC BE CC B3 B7 A8 02 56 A9 82 2B 68 6D 72 34
            Socket proxyConn = connectProxy(proxyto_ip, proxyto_port);
            if (proxyConn == null)
            {
                System.Windows.Forms.MessageBox.Show("Failed to connect the proxy conn to the server, something is wrong");
                return;
            }

            recved = proxyConn.Receive(buf);
            var firstBytes = new Byte[recved];
            Array.Copy(buf, firstBytes, recved);
            handler.Send(firstBytes);

            handler.Blocking = false;
            proxyConn.Blocking = false;

            //Send connect string
            while (true)
            {
                try
                {
                    recved = handler.Receive(buf);
                    if (recved > 0)
                    {
                        proxyConn.Send(buf, recved, SocketFlags.None);
                    }
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == (int)SocketError.WouldBlock)
                    {
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("We got disconnected errcode: {0}, handler.Recieve", e.ErrorCode));
                        return;
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Wtf happened here, handler.Recieve");
                }

                try
                {
                    recved = proxyConn.Receive(buf);
                    if (recved > 0)
                    {
                        handler.Send(buf, recved, SocketFlags.None);
                    }
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == (int)SocketError.WouldBlock)
                    {
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("We got disconnected errcode: {0}, proxyConn.Recieve", e.ErrorCode));
                        return;
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Wtf happened here, proxyConn.Recieve");
                }

                Thread.Sleep(1);
                //string hex = BitConverter.ToString(buf);
                //Console.WriteLine(hex);
            }
        }

        static Socket connectProxy(string ip, ushort port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(ip, port);
            }
            catch (Exception ex)
            {
                return null;
            }
            return socket;
        }
    }
}
