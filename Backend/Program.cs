using Backend;
using Backend.Domain.ROV_Sender;
using Backend.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register CORS service and configure allowed origins.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin() // Allow all origins
              .AllowAnyMethod()  // Allow all HTTP methods (GET, POST, etc.)
              .AllowAnyHeader(); // Allow all headers
    });
});


// Add services to the container
builder.Services.AddSingleton<CommandQueueService<Dictionary<string, object>>>();
builder.Services.AddHostedService<SDL2PoolService>(); // Background Service that Collects Controller Input and Enqueue it.
builder.Services.AddHostedService<RovCommandProcessor>(); // Background Service that Dequeue Commands, Translate it, And Send it to ROV.

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use CORS policy
app.UseCors("AllowAllOrigins");

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

