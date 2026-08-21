using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Portfolio;
using Portfolio.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Scoped is effectively singleton in a WebAssembly host - one browser, one scope.
builder.Services.AddScoped<JsInterop>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
