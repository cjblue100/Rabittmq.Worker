using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace WeatherReport.ConsoleApp
{
    class Program
    {
        private static readonly ConnectionFactory _factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672

        };
        private static readonly IConnection _connection = _factory.CreateConnection();
        private static readonly IModel _channel = _connection.CreateModel();
        private static readonly EventingBasicConsumer _consumer = new EventingBasicConsumer(_channel);
        static async Task Main(string[] args)
        {
            _channel.QueueDeclare("get-weather-queue", false, false, false, null);
            _channel.QueueDeclare("send-weather-queue", false, false, false, null);

            _consumer.Received += (model, content) =>
              {
                  var body = content.Body.ToArray();
                  var weather = Encoding.UTF8.GetString(body);
                  Console.WriteLine(weather);
              };
            _channel.BasicConsume("send-weather-queue", true, _consumer);

            Console.WriteLine("Ingrese fecha a buscar: ");
            string date = Console.ReadLine();
            GetWeather(date);


            while(true)
            {
                await Task.Delay(2000);
            }

           
           
        }

        private static void GetWeather(string date)
        {
            var body = Encoding.UTF8.GetBytes(date);
            _channel.BasicPublish("", "get-weather-queue", null, body);

        }
    }
}
