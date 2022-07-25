using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQDotNet.Domain;
using System.Text;
using System.Text.Json;

namespace RabbitMQDotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult InsertOrder(Order order)
        {
            try
            {
                #region Insert Queue
                var factory = new ConnectionFactory()
                {
                    HostName = "jackal-01.rmq.cloudamqp.com",
                    Port = 5672,
                    UserName = "nvkrsdyw",
                    Password = "e84pdS9tQTIm_gIk8TVCYH-V1EHKEAba",
                    VirtualHost = "nvkrsdyw",
                    RequestedHeartbeat = new TimeSpan(60),
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "orderQueue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = JsonSerializer.Serialize(order);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "orderQueue",
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }
                #endregion
                return Accepted(order);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocorreu um erro", ex);
                return StatusCode(500);
            }
        }
    }
}
