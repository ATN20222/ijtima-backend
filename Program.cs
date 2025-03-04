using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MeetingPlatform.Data;
using MeetingPlatform.Models;
using System.Text;
using MeetingPlatform.Data;
using MeetingPlatform.Models;
using MeetingPlatform.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register FileUploadHelper
//builder.Services.AddScoped<FileUploadHelper>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Easy Life", Version = "v1" });

    // Add JWT Bearer token support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter your token in the format **Bearer {your token}**",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
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
            new string[] {}
        }
    });
});

// Configure Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configure Identity (User and Role Management)
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
}).AddJwtBearer("Bearer", options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Register AuthService
builder.Services.AddScoped<AuthService>();
builder.Services.AddSignalR();
// CORS Configuration (if needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3001") // Allow requests from this origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable serving static files from wwwroot (default)
//app.UseStaticFiles();

//// Enable serving static files from the "Media" folder
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Media")),
//    RequestPath = "/Media"  // This makes files accessible at /Media/...
//});

// Enable CORS
app.UseCors("AllowAll");


// Enable Authentication middleware
app.UseAuthentication();

// Enable Authorization middleware
app.UseAuthorization();

// Map API Controllers
app.MapControllers();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<MeetingHub>("/meetingHub");
});
// Run the application
app.Run();
