using Microservice_AzureServiceBus.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microservice_AzureServiceBus.HostedServices
{
    public class ProductTopicConsumer2 : IHostedService
    {
        static SubscriptionClient subscriptionClient;
        private readonly IConfiguration _config;

        public ProductTopicConsumer2(IConfiguration config)
        {
            _config = config;
            var serviceBusConnection = _config.GetValue<string>("AzureServiceBus");
            subscriptionClient = new SubscriptionClient(serviceBusConnection, "stores", "stores-sub2");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("############## Starting Consumer - Topic Product Sub 1 ####################");
            ProcessMessageHandler();
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("############## Stopping Consumer - Topic Product Sub 1 ####################");
            await subscriptionClient.CloseAsync();
            await Task.CompletedTask;
        }

        public void ProcessMessageHandler()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine("### Processing Message - Topic Product Sub 1 ###");
            Console.WriteLine($"{DateTime.Now}");
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            Product _product = JsonSerializer.Deserialize<Product>(message.Body);
            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        public Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
