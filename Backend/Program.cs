using Backend;
using Backend.Domain.GUI_Updater;
using Backend.Domain.Mani_Controller;
using Backend.Domain.ROV_Controller;
using Backend.Domain.ROV_Sender;
using Backend.Infrastructure;
using Backend.Infrastructure.Interface;
using Backend.Translation;

var builder = WebApplication.CreateBuilder(args);

// Ensure logging is added
builder.Logging.ClearProviders(); // Clear default providers
builder.Logging.AddConsole();     // Add console logging
builder.Logging.AddDebug();       // (Optional) Add debug logging

// Add services to the container.
builder.Services.AddControllers();

// Register CORS service and configure AllowSpecificOrigins.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials() // Allows cookies/auth headers
              .SetIsOriginAllowed(_ => true); // Allow WebSockets from any subdomain
    });
});



// Add services to the container
builder.Services.AddSingleton<ICommandQueueService<Dictionary<string, object>>, CommandQueueService<Dictionary<string, object>>>();
builder.Services.AddSingleton<IROVController, RovController>();
builder.Services.AddSingleton<IManiController, ManiController>();
builder.Services.AddSingleton<WebSocketServer>(); // Singleton WebSocket server to handle connections
builder.Services.AddSingleton<IGUITranslationLayer, GUITranslationLayer>();
builder.Services.AddSingleton<IRovTranslationLayer, RovTranslationLayer>();

builder.Services.AddSingleton<PythonProcessManager>();
builder.Services.AddHostedService<PythonProcessService>();

builder.Services.AddSingleton<Network>();
builder.Services.AddSingleton<INetworkClient>(sp => sp.GetRequiredService<Network>());
builder.Services.AddSingleton<INetworkServer>(sp => sp.GetRequiredService<Network>());


// Background Service that Dequeue Commands, Translate it, And Send it to ROV.
builder.Services.AddHostedService<RovCommandProcessor>();
builder.Services.AddHostedService<SDL2PoolService>(); // Background Service that Collects Controller Input and Enqueue it
builder.Services.AddHostedService<DataProviderService>();
builder.Services.AddHostedService<WebSocketBackgroundService>();
builder.Services.AddHostedService<ZmqCommunicationService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add WebSocket handling to the middleware
app.UseWebSockets();  // Enable WebSockets in the app

// Use CORS policy
//app.UseCors("AllowAllOrigins");
app.UseCors("AllowSpecificOrigins");


// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

