using Blazored.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpenAutoBench_ng.Communication.Radio.Motorola.Quantar;
using OpenAutoBench_ng.Communication.Radio.Motorola.RSSRepeaterBase;
using OpenAutoBench_ng.OpenAutoBench;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlazoredToast();



// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//Task.Run(async () => WebUILauncher.WaitAndLaunchWebUI());

app.Run();
