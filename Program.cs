using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static HostingPlayground.HostingPlaygroundLogEvents;
using System;
using System.IO;
using System.Collections;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Linq;
using HostingPlayground.CompositionRoot;


namespace HostingPlayground;

class Program
{
    static async Task<int> Main(string[] args)
    {
        IHostBuilder hostbuilder = HostingPlayGroundCompositionRoot.GetHostBuilder(args);
        await BuildCommandLine()
        .UseHost(args => hostbuilder, HostingPlayGroundCompositionRoot.ActionConfigureServices)
        .UseDefaults()
        .Build()
        .InvokeAsync(args);

        IConfigurationRoot config = new ConfigurationBuilder()
            //.AddJsonFile("appsettings.json")
            //.AddEnvironmentVariables()
            .Build();
        foreach ((string key, string value) in config.AsEnumerable().OrderBy(item => item.Key))  //Linq
            Console.WriteLine($"'{key}' = '{value}'");
        return 0;
    }

    private static CommandLineBuilder BuildCommandLine()
    {
        var root = new RootCommand(@"$ dotnet run --name 'Joe'"){
            new Option<string>("--name"){
                IsRequired = true
            }
        };
        root.Handler = CommandHandler.Create<GreeterOptions, IHost>(Run);
        return new CommandLineBuilder(root);
    }

    private static void Run(GreeterOptions options, IHost host)
    {
        var serviceProvider = host.Services;
        var greeter = serviceProvider.GetRequiredService<IGreeter>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(Program));

        var name = options.Name;
        logger.LogInformation(GreetEvent, "Greeting was requested for: {name}", name);
        greeter.Greet(name);
    }

}
