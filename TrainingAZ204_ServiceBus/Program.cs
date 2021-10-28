using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace TrainingAZ204_ServiceBus
{
    class Program
    {
        protected IConfiguration Configuration { get; set; }

        private const string ArjenTopicName = "arjen";
        private const string ConnectionString = "Endpoint=sb://johngortersbns.servicebus.windows.net/;TransportType=AmqpWebSockets;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ClDhfDCAdWbTRQyboJkXcSCf1orZNsvblxy6oWknrU4=;";

        private static void Initialize()
        {
        }
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var serviceBusClient = new ServiceBusClient(ConnectionString);
            var sender = serviceBusClient.CreateSender(ArjenTopicName);

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= 3; i++)
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                    throw new Exception($"The message {i} is too large to fit in the batch.");

            try
            {
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {3} messages has been published to the topic.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await serviceBusClient.DisposeAsync();
            }
        }

        static async Task MessageHandler(ProcessMessageEventArgs args)
        {

        }
        static async Task ErrorHandler(ProcessErrorEventArgs args)
        {

        }
    }
}
