using testing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.OpenApi.Models;
using testing.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//for swagger
builder.Services.AddSwaggerGen(c =>
   {
       c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

       c.MapType<User>(() => new OpenApiSchema
       {
           Type = "object",
           Properties = new Dictionary<string, OpenApiSchema>
           {
               ["username"] = new OpenApiSchema
               {
                   Type = "string",
                   Example = new Microsoft.OpenApi.Any.OpenApiString("string")
               },
               ["password"] = new OpenApiSchema
               {
                   Type = "string",
                   Example = new Microsoft.OpenApi.Any.OpenApiString("string")
               }
           }
       });
       
       c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
       {
           Name = "Authorization",
           Type = SecuritySchemeType.ApiKey,
           Scheme = "Bearer",
           BearerFormat = "JWT",
           In = ParameterLocation.Header,
           Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                        In = ParameterLocation.Header,
                },
                new List<string>()
            }
       });
   });

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Change to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? ""))
    };
});

builder.Services.AddControllers();

//for MariaDB connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MariaDBConnection"),
    new MariaDbServerVersion(new Version(10, 5, 12))));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();