using gschatbot.api.Configuration;
using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Infrastructure.Data.Repositories;
using gschatbot.api.Infrastructure.Services;
using gschatbot.api.Services.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Options (IOptions<T>)
builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection(OllamaOptions.Section));

builder.Services.Configure<TwilioOptions>(
    builder.Configuration.GetSection(TwilioOptions.Section));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.Section));

// JWT Authentication
var jwtSection = builder.Configuration.GetSection(JwtOptions.Section);
var secretKey = jwtSection["SecretKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// LLM via HttpClient com interface
builder.Services.AddHttpClient<ILlmService, OllamaLlmService>();

// Notificação
builder.Services.AddScoped<INotificacaoService, TwilioNotificacaoService>();

// Repositórios
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IEspecialistaRepository, EspecialistaRepository>();
builder.Services.AddScoped<IEspecialidadeRepository, EspecialidadeRepository>();
builder.Services.AddScoped<IHorarioConsultaRepository, HorarioConsultaRepository>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IHistoricoMensagemRepository, HistoricoMensagemRepository>();
builder.Services.AddScoped<IPlanoAssistenciaRepository, PlanoAssistenciaRepository>();
builder.Services.AddScoped<IMetodoPagamentoRepository, MetodoPagamentoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Dispatcher de intents
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

// CORS
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
