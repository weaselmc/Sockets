using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace Sockets.ViewModels
{
    class MainPageViewModel
    {
        public ObservableCollection<string> ClientItems { get; set; }
        public ObservableCollection<string> ServerItems { get; set; }

        public string PortNumber { get; set; }
        public string ServerPort { get; set; }
        public  string ServerAddress { get; set; }
        public string ClientMessage { get; set; }

        public bool serverIsRunning = false;
        public bool clientIsConnected = false;

        private StreamSocketListener streamSocketListener;

        private StreamSocket streamSocket;
        private Stream outputStream;
        private Stream inputStream;

        public MainPageViewModel()
        {
            ClientItems = new ObservableCollection<string>();
            ServerItems = new ObservableCollection<string>();
        }

        public async void StartServer()
        {
            
            try
            {
                streamSocketListener = new StreamSocketListener();

                // The ConnectionReceived event is raised when connections are received.
                streamSocketListener.ConnectionReceived += StreamSocketListener_ConnectionReceived;

                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await streamSocketListener.BindServiceNameAsync(PortNumber);                
                serverIsRunning = true;
                ServerItems.Add(string.Format("server is listening on port {0} ...", PortNumber));
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                ServerItems.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }
        private async void StreamSocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            string request;
            using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                request = await streamReader.ReadLineAsync();
            }

            await CoreApplication.MainView.CoreWindow.
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, 
                () => ServerItems.Add(string.Format("server received the request: \"{0}\"", request)));

            // Echo the request back as the response.
            using (Stream ServeroutputStream = args.Socket.OutputStream.AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(ServeroutputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.
                RunAsync(CoreDispatcherPriority.Normal, 
                () => ServerItems.Add(string.Format("server sent back the response: \"{0}\"", request)));
                       
        }

        public async void StopServer()
        {
            streamSocketListener.Dispose();
            serverIsRunning = false;
            await CoreApplication.MainView.CoreWindow.
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => ServerItems.Add("server closed its socket"));
        }

        public async void StartClient()
        {
            try
            {
                // Create the StreamSocket and establish a connection to the echo server.
                streamSocket = new StreamSocket();
                // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                var hostName = new Windows.Networking.HostName(ServerAddress);

                ClientItems.Add(string.Format("client is trying to connect to {0}:{1} ...", ServerAddress, ServerPort));

                await streamSocket.ConnectAsync(hostName, ServerPort);
                clientIsConnected = true;
                ClientItems.Add("client connected");
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                ClientItems.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        public async void SendClientMessage()
        {
            try {
                // Send a request to the echo server.
                //string request = "Hello, World!";
                if(outputStream == null)
                    outputStream = streamSocket.OutputStream.AsStreamForWrite();
                
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(ClientMessage);
                    await streamWriter.FlushAsync();
                }
                

                ClientItems.Add(string.Format("client sent the request: \"{0}\"", ClientMessage));

                // Read data from the echo server.
                string response;
                if (inputStream == null)
                    inputStream = streamSocket.InputStream.AsStreamForRead();                
                using (StreamReader streamReader = new StreamReader(inputStream))
                {
                    response = await streamReader.ReadLineAsync();
                }

                ClientItems.Add(string.Format("client received the response: \"{0}\" ", response));
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                ClientItems.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }

        public async void CloseClient()
        {
            //await inputStream.FlushAsync();
            inputStream.Dispose();
            //await outputStream.FlushAsync();
            outputStream.Dispose();
            streamSocket.Dispose();
            clientIsConnected = false;
            ClientItems.Add("client closed its socket");
        }
        
    }
}
