using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using BlogSharp.Entities;
using BlogSharp.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure environment variables and app settings
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Add Identity with default token providers
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = true; // Example: Ensure unique emails
        options.Password.RequireDigit = true;  // Example: Enforce strong passwords
    })
    .AddEntityFrameworkStores<BlogDbContext>()
    .AddDefaultTokenProviders() // Ensures token-based features like password reset work
    .AddApiEndpoints(); // Enables Identity API endpoints

// Configure EF Core with PostgreSQL
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(BlogDbContext).Assembly.GetName().Name))
);

// Redis for cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "YourRedisInstanceName";
});

// RabbitMQ
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

// API Controllers
builder.Services.AddControllers();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
