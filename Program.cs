using Microsoft.EntityFrameworkCore;
using CocktailService.Models;
using CocktailService.Importers;
using CocktailService.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 📌 PostgreSQL
builder.Services.AddDbContext<CocktailDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddAuthorization();

// 📌 CORS (per ora aperto → puoi chiuderlo in produzione)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 📌 HttpContextAccessor → utile per recuperare userId/token
builder.Services.AddHttpContextAccessor();

// 📌 HttpClient CLIENTS (Controllers → ImportService)
builder.Services.AddHttpClient<ApiIngredientsClient>(client =>
{
    client.BaseAddress = new Uri("http://cocktail-import-service");
});
builder.Services.AddHttpClient<ApiCocktailsClient>(client =>
{
    client.BaseAddress = new Uri("http://cocktail-import-service");
});
builder.Services.AddHttpClient<ApiCocktailIngredientsClient>(client =>
{
    client.BaseAddress = new Uri("http://cocktail-import-service");
});

// 📌 HttpClient IMPORTERS (Sync Service)
builder.Services.AddHttpClient<ImportIngredientsClient>(client =>
{
    client.BaseAddress = new Uri("http://cocktail-import-service");
});
builder.Services.AddHttpClient<ImportCocktailsClient>(client =>
{
    client.BaseAddress = new Uri("http://cocktail-import-service");
});
builder.Services.AddHttpClient<ImportCocktailIngredientsClient>(client =>
{
    client.BaseAddress = new Uri("http://cocktail-import-service");
});

// 📌 Sync Service
builder.Services.AddScoped<CocktailImportSyncService>();

var app = builder.Build();

// 🛎️ AUTO-MIGRATION E CREAZIONE DATABASE
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CocktailDbContext>();
    db.Database.Migrate(); // Applica migrazioni → crea il db se non esiste
}

// 🚦 Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");       // 💡 IMPORTANTISSIMO → prima di Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🍹 CocktailService avviato su: " + builder.Configuration["ASPNETCORE_URLS"]);
app.Run();
