using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMq_HelloWorld
{
    class Program
    {
        static bool espera = true;
        static void Main(string[] args)
        {
            Task.Run(() => ReceiveTask("DOS"));
            Task.Run(() => ReceiveTask("UNO"));

            var factory1 = new ConnectionFactory() { HostName = "localhost" };
            using (var connection1 = factory1.CreateConnection())
            using (var channel = connection1.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                Console.WriteLine(" Press [enter] to continue.");
                Console.ReadLine();

                string message = "Hello World!";
                var body1 = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body1);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
            espera = false;
            Task.Delay(100).Wait();
        }
        static void ReceiveTask(string id)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" On {id} [x] Received {message}");
                };
                channel.BasicConsume(queue: "hello",
                                     autoAck: true,
                                     consumer: consumer);
                while (espera)
                {
                    Task.Delay(20).Wait();
                }
            }

        }
    }
}
