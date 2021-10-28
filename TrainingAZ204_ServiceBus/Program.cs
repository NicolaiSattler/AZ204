using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TrainingAZ204_ServiceBus
{
    class Program
    {
        protected IConfiguration Configuration { get; set; }

        private const string MessageTopic = "john";
        private const string ReceiverTopic = "nicolai";
        private const string Subscriptions = "sub";
        private const string ConnectionString = "Endpoint=sb://johngortersbns.servicebus.windows.net/;TransportType=AmqpWebSockets;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ClDhfDCAdWbTRQyboJkXcSCf1orZNsvblxy6oWknrU4=;";

        private static ServiceBusClient ServiceBusClient { get; set; }

        private static void Initialize()
        {
        }
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ServiceBusClient = new ServiceBusClient(ConnectionString);
            var options = new ServiceBusProcessorOptions();
            var processor = ServiceBusClient.CreateProcessor(ReceiverTopic, MessageTopic, options);

            try
            {
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                await processor.StartProcessingAsync();

                Console.WriteLine($"A batch of {3} messages has been published to the topic.");

                while (1 < 2)
                {

                }
                await processor.StopProcessingAsync();
            }
            finally
            {
                await ServiceBusClient.DisposeAsync();
            }
        }

        static async Task SendMessage(string message)
        {
            var sender = ServiceBusClient.CreateSender(MessageTopic);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            try
            {
                messageBatch.TryAddMessage(new ServiceBusMessage(message));

                await sender.SendMessagesAsync(messageBatch);
            }
            finally
            {
                await sender.DisposeAsync();
            }

        }
        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            var mutation = new string(body.Reverse().ToArray());

            await SendMessage(mutation);
            await args.CompleteMessageAsync(args.Message);
        }
        static async Task ErrorHandler(ProcessErrorEventArgs args)
        {

        }
    }
}
