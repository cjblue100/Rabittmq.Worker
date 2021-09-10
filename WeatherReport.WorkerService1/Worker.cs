using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeatherReport.WorkerService1.Model;
namespace WeatherReport.WorkerService1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        //private readonly HttpClient _httpClient;

        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly EventingBasicConsumer _consumer;
        public Worker(ILogger<Worker> logger )
        {
            _logger = logger;
           // _httpClient = new HttpClient();
            //_httpClient.BaseAddress = new Uri("http://localhost:59165/");
             var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare("get-weather-queue", false, false, false, null);
            _channel.QueueDeclare("send-weather-queue", false, false, false, null);
            _consumer = new EventingBasicConsumer(_channel);
        }

        public  override Task StartAsync(CancellationToken cancellationToken)
        {
            _consumer.Received += async (model, content) =>
            {
                var body = content.Body.ToArray();
                var date = Encoding.UTF8.GetString(body);
                var result = await GetWeatherAsync(date);
                var message = JsonConvert.SerializeObject(result);
                var body2 = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(string.Empty, "send-weather-queue", null, body2);

                //var json = Encoding.UTF8.GetString(body);
                //var basket = JsonConvert.DeserializeObject<string>(json);
                //var result=  await _httpClient.GetStringAsync($"WeatherForecast/{basket}");
                ////var coso = JsonConvert.DeserializeObject<Model.WeatherReport>(result.ToString());
                //Console.WriteLine("heyyyyyy aqyuuuuuu "+result.ToString());
                
            };

            _channel.BasicConsume("get-weather-queue", true, _consumer);
            return Task.CompletedTask;
        }

        private async Task<Model.WeatherReport> GetWeatherAsync(string date)
        {
            await Task.Delay(2000);
            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.GetStringAsync($"http://localhost:59165/WeatherForecast/{date}");
                return JsonConvert.DeserializeObject<Model.WeatherReport>(result);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
