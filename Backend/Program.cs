using Backend;
using SDL2;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);


// Create a shared event queue and register it as a singleton
var rovQueue = new BlockingCollection<SDL.SDL_Event>(100); // Set a max capacity
builder.Services.AddSingleton(rovQueue);

// Add services to the container
builder.Services.AddHostedService<SDL2EventLoopService>(); // SDL event loop
builder.Services.AddHostedService<Worker>(); // Worker service for joystick processing

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();

