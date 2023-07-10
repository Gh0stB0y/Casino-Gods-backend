using CasinoGodsAPI.Data;
using CasinoGodsAPI.Hubs.TablesHubs;
using CasinoGodsAPI.Services;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Org.BouncyCastle.Asn1.X509.Qualified;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Concurrent;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

var RedisConnection = builder.Configuration.GetConnectionString("RedisConnection");
builder.Services.AddSingleton<IConnectionMultiplexer>(opt =>
{
    var configurationOptions = ConfigurationOptions.Parse(RedisConnection);
    var connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
    for (int i = 1; i < 4; i++)
    {
        var database = connectionMultiplexer.GetDatabase();
        database.Execute("SELECT", i);
    }
    return connectionMultiplexer;
});

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddHostedService<LobbyService>();
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();   
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {Token}\")",
        In =ParameterLocation.Header,
        Name="Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
    };
    });
builder.Services.AddDbContext<CasinoGodsDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("FullStackConnectionString")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Allow sending credentials (e.g., cookies) with the request
    });
});
//
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed((host) => true));
app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
//HUB settings

//
app.MapControllers();
app.UseEndpoints(endpoints => {

    

    endpoints.MapHub<BacarratLobby>("/BacarratLobby");
    endpoints.MapHub<BlackjackLobby>("/BlackJackLobby");
    endpoints.MapHub<RouletteLobby>("/RouletteLobby");
    endpoints.MapHub<DragonTigerLobby>("/DragonTigerLobby");
    endpoints.MapHub<WarLobby>("/WarLobby");

    endpoints.MapHub<BacarratTables1>("/BacarratTables1");
    endpoints.MapHub<BacarratTables2>("/BacarratTables2");
    endpoints.MapHub<BacarratTables3>("/BacarratTables3");

    endpoints.MapHub<BlackjackTables1>("/BlackjackTables1");
    endpoints.MapHub<BlackjackTables2>("/BlackjackTables2");
    endpoints.MapHub<BlackjackTables3>("/BlackjackTables3");

    endpoints.MapHub<DragonTigerTables1>("/Dragon TigerTables1");
    endpoints.MapHub<DragonTigerTables2>("/Dragon TigerTables2");
    endpoints.MapHub<DragonTigerTables3>("/Dragon TigerTables3");

    endpoints.MapHub<RouletteTables1>("/RouletteTables1");
    endpoints.MapHub<RouletteTables2>("/RouletteTables2");
    endpoints.MapHub<RouletteTables3>("/RouletteTables3");

    endpoints.MapHub<WarTables1>("/WarTables1");
    endpoints.MapHub<WarTables2>("/WarTables2");
    endpoints.MapHub<WarTables3>("/WarTables3");
});
Init();

app.Run();

void Init()
{
   

}


