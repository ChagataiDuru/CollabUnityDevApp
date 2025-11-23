using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UnityDevHub.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Auto-apply migrations
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try 
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
    }
}

// Authentication
builder.Services.AddScoped<UnityDevHub.API.Services.IAuthService, UnityDevHub.API.Services.AuthService>();
builder.Services.AddScoped<UnityDevHub.API.Services.IVcsService, UnityDevHub.API.Services.VcsService>();
builder.Services.AddScoped<UnityDevHub.API.Services.INotificationService, UnityDevHub.API.Services.NotificationService>();
builder.Services.AddScoped<UnityDevHub.API.Services.IDocumentationService, UnityDevHub.API.Services.DocumentationService>();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_secret_key_must_be_long_enough"))
        };
    });

builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UnityDevHub API", Version = "v1" });

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
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
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200", "http://localhost")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<UnityDevHub.API.Hubs.ProjectHub>("/hubs/project");
app.MapHub<UnityDevHub.API.Hubs.WhiteboardHub>("/hubs/whiteboard");

app.Run();
