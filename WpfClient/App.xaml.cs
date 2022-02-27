using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

using System.Windows;
using Test;

//using Test;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(typeof(MainWindow));

            services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));
            //The gRPC client type is registered as transient with dependency injection (DI). 
            services.AddGrpcClient<Tester.TesterClient>(o =>
            {
                o.Address = new Uri("https://localhost:5001");
            }).EnableCallContextPropagation();///(o => o.SuppressContextNotFoundErrors = true);
            //By default, EnableCallContextPropagation raises an error if the client is used outside the context of a gRPC call.
            //The error is designed to alert you that there isn't a call context to propagate. If you want to use the client outside of a call context,
            //suppress the error when the client is configured with SuppressContextNotFoundErrors:


            #region named clients
            services.AddGrpcClient<Tester.TesterClient>("Tester", o =>
            {
                o.Address = new Uri("https://localhost:5001");
            });

            services.AddGrpcClient<Tester.TesterClient>("AuthenticatedTester", o =>
            {
                o.Address = new Uri("https://localhost:5001");
            })
            .ConfigureChannel(o =>
            {
                //o.Credentials = new CustomCredentials(); //if you have custom configurations for multible users
            });
            #endregion
        }
    }
}
