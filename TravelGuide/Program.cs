using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TravelGuide.Data;
using TravelGuide.Repository.Posts;
using TravelGuide.Repository.User;
using TravelGuide.Services;
 
var builder = WebApplication.CreateBuilder(args);
 
 
// Add services to the container.
 
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TravelGuide API", Version = "v1" });
 
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});
 
//Registering the HttpClient
builder.Services.AddHttpClient();
 
//Registering the WebRootPath
builder.WebHost.UseWebRoot("wwwroot");
 
// Add services to the container.
builder.Services.AddControllersWithViews();
 
//Registering the GooglePlacesService
builder.Services.AddScoped<IPlacesService, GeoapifyPlacesService>();
 
//Registering the UserRepository
builder.Services.AddScoped<IUserRepository, UserRepository>();
 
//Registering the JWT Token Service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
 
 
// Registering the EmailService
builder.Services.AddScoped<IEmailService, EmailService>();
 
//Registering the PostService
builder.Services.AddScoped<IPostService, PostService>();
 
//Registering the PostRepository
builder.Services.AddScoped<IPostRepository, PostRepository>();
 
// Registering the DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
 
 
// Registering the JWT authentication
builder.Services.AddAuthentication("Bearer")
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>  // this is only for the development purpose in the production this will cause the security issue.
    {
        policy.SetIsOriginAllowed(origin =>
            new Uri(origin).Host == "localhost") 
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
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}
else
{
    app.UseSwagger();  // this will not be needed in the production
    app.UseSwaggerUI();
}
 
// Enable static files middleware
app.UseStaticFiles();
 
app.UseHttpsRedirection();
 
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowLocalhost");
 
app.MapControllers();
 
app.Run();
