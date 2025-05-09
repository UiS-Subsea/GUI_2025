using Backend.Domain.GUI_Updater;
using Backend.Domain.ROV_Sender;
using Backend.Infrastructure;
using Backend.Infrastructure.Interface;
using Backend.Translation;
using Backend.Logging;
using Backend.Domain.Controllers.ROV_Controller;
using Backend.Domain.Controllers.Mani_Controller;
using Backend.Domain.Controllers;

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
builder.Services.AddSingleton<IModeService, ModeService>(); // "global variable" to tell backend what Drive Mode it is.
builder.Services.AddSingleton<WebSocketServer>(); // Singleton WebSocket server to handle connections.
builder.Services.AddSingleton<IROVController, RovController>(); // Handles input from ROV controller.
builder.Services.AddSingleton<IManiController, ManiController>(); // Handles input from Manipulator Controller.

builder.Services.AddSingleton<IGUITranslationLayer, GUITranslationLayer>(); // Translate data into GUI formate data.
builder.Services.AddSingleton<IRovTranslationLayer, RovTranslationLayer>(); // Translate Generic command data into ROV format Data.
builder.Services.AddSingleton<LoggerService>();  //Logging

builder.Services.AddSingleton<PythonProcessManager>();
builder.Services.AddHostedService<PythonProcessService>();

builder.Services.AddSingleton<Network>(); // Handles the Network connection to the ROV.
builder.Services.AddSingleton<INetworkClient>(sp => sp.GetRequiredService<Network>()); // Sends the data to ROV
builder.Services.AddSingleton<INetworkServer>(sp => sp.GetRequiredService<Network>()); // Receives Sensor Data from ROV.


// Background Services:
builder.Services.AddHostedService<RovCommandService>(); // Service that Dequeue Commands, Translate it, And Send it to ROV.
builder.Services.AddHostedService<ControllerEventService>(); // Service that Collects Controller Input and Enqueue it.
builder.Services.AddHostedService<DataProviderService>(); // Service that Translate and send it to GUI.
builder.Services.AddHostedService<WebSocketService>(); // Websocket that sends and receives data between Backend and Frontend.
builder.Services.AddHostedService<ZmqCommunicationService>(); // Receives ROV controlling from the Python process and Enqueue it.

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add WebSocket handling to the middleware
app.UseWebSockets();  // Enable WebSockets in the app

// Use CORS policy
app.UseCors("AllowSpecificOrigins");


app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

