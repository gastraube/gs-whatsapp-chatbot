using gschatbot.api.Data;
using gschatbot.api.Services;
using gschatbot.api.Services.Handlers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// HttpClient
builder.Services.AddHttpClient<LlmService>();
builder.Services.AddHttpClient();

// Services
builder.Services.AddScoped<TwilioService>();
builder.Services.AddScoped<LlmService>();
builder.Services.AddScoped<IntentDispatcher>();

// Registra todos os handlers dinamicamente
var handlerType = typeof(IIntentHandler);
var handlers = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(s => s.GetTypes())
    .Where(p => handlerType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

foreach (var handler in handlers)
{
    builder.Services.AddScoped(handler);
}

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (se precisar)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();