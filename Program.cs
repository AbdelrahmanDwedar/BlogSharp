using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using BlogSharp.Entities;
using BlogSharp.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
    })
    .AddEntityFrameworkStores<BlogDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();

builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(BlogDbContext).Assembly.GetName().Name))
);

// Add Redis caching service
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "BlogSharpRedisInstance";
});

// Register Redis-based cache service
builder.Services.AddSingleton<IRedisCache, RedisCache>();

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var rabbitMqConnectionString =
        builder.Configuration.GetConnectionString("RabbitMQConnection")
        ?? "default_rabbitmq_connection_string";
    var factory = new ConnectionFactory()
    {
        Uri = new Uri(rabbitMqConnectionString),
        DispatchConsumersAsync = true
    };
    return factory;
});

builder.Services.AddTransient<IConnection>(sp =>
{
    var connectionFactory = sp.GetRequiredService<IConnectionFactory>();
    return connectionFactory.CreateConnection();
});

builder.Services.AddTransient<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateModel();
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
