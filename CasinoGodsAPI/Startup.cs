using CasinoGodsAPI.Data;
using CasinoGodsAPI.Services;
using CasinoGodsAPI.TablesModel;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;


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
//builder.Services.AddHostedService<TableService>();

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
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("FullStackConnectionString"));
},ServiceLifetime.Scoped);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

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
});
Init(app.Services);

app.Run();

void Init(IServiceProvider serviceProvider)
{
    using (var scope = serviceProvider.CreateScope())
    {
        var casinoGodsDbContext = scope.ServiceProvider.GetRequiredService<CasinoGodsDbContext>();

        var recordsToDelete = casinoGodsDbContext.ActiveTables.ToList();
        casinoGodsDbContext.ActiveTables.RemoveRange(recordsToDelete);
        casinoGodsDbContext.SaveChanges();
    }
}

