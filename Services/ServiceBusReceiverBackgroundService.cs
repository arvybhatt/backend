using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class ServiceBusReceiverBackgroundService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceBusReceiverBackgroundService> _logger;

        public ServiceBusReceiverBackgroundService(IConfiguration configuration, ILogger<ServiceBusReceiverBackgroundService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Try to get connection string from Key Vault first, then fallback to configuration
            var connectionString = _configuration["ServiceBusConnectionString"] ?? 
                                  _configuration["AzureServiceBus:ConnectionString"];
            var queueName = _configuration["AzureServiceBus:QueueName"];

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(queueName))
            {
                _logger.LogWarning("Azure Service Bus connection string or queue name is not set.");
                return;
            }

            await using var client = new ServiceBusClient(connectionString);
            var processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += async args =>
            {
                var body = args.Message.Body.ToString();
                _logger.LogInformation($"Received message: {body}");
                ServiceBusMessageStore.Messages.Enqueue(body);
                await args.CompleteMessageAsync(args.Message);
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Service Bus error");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);

            // Keep running until stopped
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await processor.StopProcessingAsync();
        }
    }
}
