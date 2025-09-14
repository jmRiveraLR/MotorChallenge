using MotorControlGateway.Services;
using MotorControlGateway.Hubs;

var builder = WebApplication.CreateBuilder(args);

//servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // front end location ,change if it changes
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Registramos el simulador como singleton
builder.Services.AddSingleton<MotorSimulator>();

// Registramos el broadcaster

builder.Services.AddHostedService<TelemetryBroadcaster>();

var app = builder.Build();

// --- Middlewares ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();
app.MapHub<MotorHub>("/motorHub"); // Ruta del hub SignalR

app.Run();
