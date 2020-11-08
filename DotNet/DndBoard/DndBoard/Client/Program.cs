using System;
using System.Net.Http;
using System.Threading.Tasks;
using DndBoard.Client.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DndBoard.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddTransient<CanvasMapRenderer>();
            builder.Services.AddTransient<ChatHubManager>();

            await builder.Build().RunAsync();
        }
    }
}
