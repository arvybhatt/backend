
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Key Vault with Service Principal Authentication
var keyVaultName = builder.Configuration["KeyVault:Name"];
if (!string.IsNullOrEmpty(keyVaultName))
{
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    
    // Get Service Principal credentials from configuration
    var tenantId = builder.Configuration["ServicePrincipal:TenantId"];
    var clientId = builder.Configuration["ServicePrincipal:ClientId"];
    var clientSecret = builder.Configuration["ServicePrincipal:ClientSecret"];
    
    if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
    {
        // Use ClientSecretCredential for Service Principal authentication
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        builder.Configuration.AddAzureKeyVault(keyVaultUri, credential);
        Console.WriteLine($"Configured Key Vault authentication using Service Principal");
    }
    else
    {
        // Fallback to DefaultAzureCredential if Service Principal details are not provided
        builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
        Console.WriteLine($"Configured Key Vault authentication using DefaultAzureCredential");
    }
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { 
        Title = "Service Bus API", 
        Version = "v1",
        Description = "API for sending and receiving Azure Service Bus messages"
    });
});
builder.Services.AddControllers();
// Register the Azure Service Bus background service from Services namespace
builder.Services.AddHostedService<Backend.Services.ServiceBusReceiverBackgroundService>();
builder.Services.AddScoped<Backend.Services.ServiceBusSenderService>();

// Add DbContext for Todo application
builder.Services.AddDbContext<TodoDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    try
    {
        // Using MySQL with username and password
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions => 
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
        });
        Console.WriteLine("Database connection configured successfully with MySQL");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error configuring database connection: {ex.Message}");
        // In development, we could fall back to an in-memory database
        // For now, we'll rethrow to see the error
        throw;
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Map controllers
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<TodoDbContext>();
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"Using connection string: {connectionString}");
        
        // Test the connection first
        Console.WriteLine("Testing database connection...");
        var canConnect = dbContext.Database.CanConnect();
        Console.WriteLine($"Database connection test: {(canConnect ? "SUCCESS" : "FAILED")}");
        
        if (canConnect)
        {
            // Check if database exists and create if needed
            var dbExists = dbContext.Database.EnsureCreated();
            Console.WriteLine($"Database created: {dbExists}, Database name: {dbContext.Database.GetDbConnection().Database}");
            
            // Log database provider and connection information
            Console.WriteLine($"Database provider: {dbContext.Database.ProviderName}");
            Console.WriteLine($"Connection string being used: {dbContext.Database.GetConnectionString()}");
        }
        else
        {
            Console.WriteLine("Cannot connect to database. Please check your connection string and network/firewall settings.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while creating the database: {ex.Message}");
        Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

app.Run();

