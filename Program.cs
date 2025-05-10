using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
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
var keyBytes   = Encoding.ASCII.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.SaveToken            = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSection["Issuer"],
            ValidAudience            = jwtSection["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization();

// 📌 CORS (per ora aperto → puoi chiuderlo in produzione)
builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                  .AllowAnyHeader()
                                  .AllowAnyMethod());
});

//HttpClient + Handlers 
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<ClearAuthHeaderHandler>(); // pulisce eventuale header utente
builder.Services.AddTransient<JwtServiceHandler>();       // firma token role=Service

// --- CLIENTS usati dai controller (export da Import‑Service) ---
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

// --- CLIENT tipizzato verso Search‑Service (per trigger reload) ---
builder.Services.AddHttpClient<SearchSyncClient>(c =>
{
    c.BaseAddress = new Uri("http://search-service");
})
.AddHttpMessageHandler<JwtServiceHandler>();

// --- CLIENTS usati dall’import “sincronizzato” (background o /import/all) ---
builder.Services.AddHttpClient<ImportIngredientsClient>(c =>
{
    c.BaseAddress = new Uri("http://cocktail-import-service");
})
.AddHttpMessageHandler<ClearAuthHeaderHandler>()
.AddHttpMessageHandler<JwtServiceHandler>();

builder.Services.AddHttpClient<ImportCocktailsClient>(c =>
{
    c.BaseAddress = new Uri("http://cocktail-import-service");
})
.AddHttpMessageHandler<ClearAuthHeaderHandler>()
.AddHttpMessageHandler<JwtServiceHandler>();

builder.Services.AddHttpClient<ImportCocktailIngredientsClient>(c =>
{
    c.BaseAddress = new Uri("http://cocktail-import-service");
})
.AddHttpMessageHandler<ClearAuthHeaderHandler>()
.AddHttpMessageHandler<JwtServiceHandler>();

// 📌 Sync Service
builder.Services.AddScoped<CocktailImportSyncService>();
builder.Services.AddScoped<CocktailManager>();
builder.Services.AddScoped<ImportFacadeService>();

var app = builder.Build();

// 🛎️ AUTO-MIGRATION E CREAZIONE DATABASE
using (var scope = app.Services.CreateScope())
{
    var db   = scope.ServiceProvider.GetRequiredService<CocktailDbContext>();
    int max  = 10;
    for (int attempt = 1; attempt <= max; attempt++)
    {
        try
        {
            db.Database.Migrate();
            Console.WriteLine("✅ Migration completata.");
            break;
        }
        catch when (attempt < max)
        {
            Console.WriteLine($"⏳ DB non pronto… ritento ({attempt}/{max})");
            Thread.Sleep(2000);
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

Console.WriteLine("🍹 CocktailService avviato.");
app.Run();
