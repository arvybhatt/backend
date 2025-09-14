using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class ServiceBusSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceBusSenderService> _logger;

        public ServiceBusSenderService(IConfiguration configuration, ILogger<ServiceBusSenderService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

public async Task SendTestMessageAsync(string message)
{
    // Try to get connection string from Key Vault first, then fallback to configuration
    var connectionString = _configuration["ServiceBusConnectionString"] ?? 
                          _configuration["AzureServiceBus:ConnectionString"];
    var queueName = _configuration["AzureServiceBus:QueueName"];            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(queueName))
            {
                _logger.LogWarning("Azure Service Bus connection string or queue name is not set.");
                return;
            }

            await using var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(queueName);
            var serviceBusMessage = new ServiceBusMessage(message);
            await sender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation($"Sent test message: {message}");
        }
    }
}
