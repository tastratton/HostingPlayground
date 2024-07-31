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
using System.Collections;

namespace HostingPlayground;

class Program
{
    // for dependency injection container
    private static Func<string[], IHostBuilder> hostbuilder = args => Host.CreateDefaultBuilder(args)
        .UseDefaultServiceProvider((context, options) => { options.ValidateScopes = true; });

    private static Action<IHostBuilder> ActionConfigureHost = host =>
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IGreeter, Greeter>();
        });
    };

    // command line
    static async Task Main(string[] args)
    {
        /*
         * Console.WriteLine("GetEnvironmentVariables: ");
        foreach (DictionaryEntry de in Environment.GetEnvironmentVariables()) 
        {
            Console.WriteLine("  {0} = {1}", de.Key, de.Value);
        };
        */
        
        
        await BuildCommandLine()
        .UseHost(args => hostbuilder(args), ActionConfigureHost)
        .UseDefaults()
        .Build()
        .InvokeAsync(args);
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
