using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using CocktailService.Models;
using CocktailService.Importers;
using CocktailService.Clients;
using CocktailService.Services;

var builder = WebApplication.CreateBuilder(args);

// 📌 Controllers
builder.Services.AddControllers();

// 📌 Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Cocktail Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 📌 PostgreSQL
builder.Services.AddDbContext<CocktailDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 📌 JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.ASCII.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrService", policy =>
        policy.RequireRole("Admin", "Service"));
});

// 📌 CORS (per ora aperto → puoi chiuderlo in produzione)
builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                  .AllowAnyHeader()
                                  .AllowAnyMethod());
});

// HttpClient + Handlers 
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<ClearAuthHeaderHandler>();
builder.Services.AddTransient<JwtServiceHandler>();

// --- CLIENTS usati dai controller (export da Import-Service) ---
builder.Services.AddHttpClient<ApiIngredientsClient>(c =>
{
    c.BaseAddress = new Uri("http://cocktail-import-service");
})
.AddHttpMessageHandler<ClearAuthHeaderHandler>()
.AddHttpMessageHandler<JwtServiceHandler>();

builder.Services.AddHttpClient<ApiCocktailsClient>(c =>
{
    c.BaseAddress = new Uri("http://cocktail-import-service");
})
.AddHttpMessageHandler<ClearAuthHeaderHandler>()
.AddHttpMessageHandler<JwtServiceHandler>();

builder.Services.AddHttpClient<ApiCocktailIngredientsClient>(c =>
{
    c.BaseAddress = new Uri("http://cocktail-import-service");
})
.AddHttpMessageHandler<ClearAuthHeaderHandler>()
.AddHttpMessageHandler<JwtServiceHandler>();

// --- CLIENT tipizzato verso Search-Service (per trigger reload) ---
builder.Services.AddHttpClient<SearchSyncClient>(c =>
{
    c.BaseAddress = new Uri("http://search-service");
})
.AddHttpMessageHandler<JwtServiceHandler>();

var app = builder.Build();

// Recupero il logger di Program
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// 🛎️ AUTO-MIGRATION E CREAZIONE DATABASE
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CocktailDbContext>();
    var logCtx = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    int max = 10;
    for (int attempt = 1; attempt <= max; attempt++)
    {
        try
        {
            db.Database.Migrate();
            logCtx.LogInformation("[CocktailService] ✅ Migration completata.");
            break;
        }
        catch (Exception ex) when (attempt < max)
        {
            logCtx.LogWarning(
                "[CocktailService] ⏳ DB non pronto… ritento ({Attempt}/{MaxAttempts})",
                attempt,
                max
            );
            await Task.Delay(2000);
        }
        catch (Exception ex)
        {
            logCtx.LogError(
                ex,
                "[CocktailService] ❌ Errore irreversibile durante la migrazione"
            );
            throw;
        }
    }
}

// 🚦 Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");   // 💡 IMPORTANTISSIMO → prima di Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Logging della configurazione del SearchSyncClient
logger.LogDebug("[CocktailService] 🧪 SearchSyncClient configurato con JWT handler.");

// Avvio dell'applicazione
logger.LogInformation("[CocktailService] 🍹 CocktailService avviato.");

app.Run();
