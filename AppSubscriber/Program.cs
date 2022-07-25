using AppSubscriber.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

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

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        try
        {
            var body = ea.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            var order = JsonSerializer.Deserialize<Order>(message);
            Console.WriteLine($"Order number: " +
                            $"{order.OrderNumber}, Name Product: {order.ItemName}, Price: {order.Price.ToString("c")}");
            channel.BasicAck(ea.DeliveryTag, false); // leu a mensagem e NÃO coloca o item na fila, porque já leu
        }
        catch (Exception ex)
        {
            channel.BasicNack(ea.DeliveryTag, false, true); // se der pal, recoloca o item na fila
        }
    };
    channel.BasicConsume(queue: "orderQueue",
                         autoAck: false, // se tiver como false só processa a
                                         // mensagem se realmente ela foi lida, com Nack e Ack.
                         consumer: consumer);
    Console.WriteLine("Exit");
    Console.ReadLine();
}
