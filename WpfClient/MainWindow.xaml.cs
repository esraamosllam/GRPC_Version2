using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Test;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Tester.TesterClient tester;
        private readonly AppSettings settings;
        //client is registered as transient first then injected in this class constructor
        public MainWindow(Tester.TesterClient tester,
                          IOptions<AppSettings> settings)
        {
            InitializeComponent();

            this.tester = tester;
            
            this.settings = settings.Value;
        }

        private async void btn1_Click(object sender, RoutedEventArgs e)
        {
            btn1.Background = Brushes.LightBlue;
            var x = await tester.SayHelloUnaryAsync(new HelloRequest { Name="esraa"});
            btn1.Content = x.Message;
        }
        //if we want to use named client
        //public MainWindow(GrpcClientFactory grpcClientFactory)
        //{
        //    this.tester = grpcClientFactory.CreateClient<Tester.TesterClient>("AuthenticatedTester");
        //}
    }
}
