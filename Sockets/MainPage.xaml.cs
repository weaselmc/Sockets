using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.Sockets;
using Sockets.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sockets
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MainPageViewModel viewModel;
        
        public MainPage()
        {
            viewModel = new MainPageViewModel();
            this.InitializeComponent();
        }

        private void startClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.clientIsConnected)
            {
                viewModel.CloseClient();
                startClientButton.Content = "Start Client";
            }
            else
            {
                viewModel.StartClient();
                startClientButton.Content = "Close Connection";
            }
        }

        private void startServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.serverIsRunning)
            {
                viewModel.StopServer();
                startServerButton.Content = "Start Server";
            }
            else
            {
                viewModel.StartServer();
                startServerButton.Content = "Stop Server";
            }
        }

        private void sendClientMsgButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SendClientMessage();
        }
    }
}
