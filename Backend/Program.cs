using Backend;
using Backend.Domain.GUI_Updater;
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

// Register CORS service and configure allowed origins.
/*builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin() // Allow all origins
              .AllowAnyMethod()  // Allow all HTTP methods (GET, POST, etc.)
              .AllowAnyHeader() // Allow all headers
    });
});
*/
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Specify your frontend URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials() // Allows cookies/auth headers
              .SetIsOriginAllowed(_ => true); // Allow WebSockets from any subdomain
    });
});



// Add services to the container
builder.Services.AddSingleton<CommandQueueService<Dictionary<string, object>>>();
builder.Services.AddSingleton<WebSocketServer>(); // Singleton WebSocket server to handle connections
builder.Services.AddSingleton<GUITranslationLayer>();
builder.Services.AddSingleton<RovTranslationLayer>();
builder.Services.AddSingleton<INetworkServer, Network>(sp =>
{
    return ActivatorUtilities.CreateInstance<Network>(sp, true, "0.0.0.0", 6901);
});
builder.Services.AddSingleton<INetworkClient, Network>(sp =>
{
    return ActivatorUtilities.CreateInstance<Network>(sp, false, "127.0.0.1", 6900);
});

// Background Service that Dequeue Commands, Translate it, And Send it to ROV.
builder.Services.AddHostedService<RovCommandProcessor>();
builder.Services.AddHostedService<SDL2PoolService>(); // Background Service that Collects Controller Input and Enqueue it
builder.Services.AddHostedService<DataProviderService>();
builder.Services.AddHostedService<WebSocketBackgroundService>();

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

