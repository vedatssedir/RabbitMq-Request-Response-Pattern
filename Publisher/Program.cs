using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

ConnectionFactory connectionFactory = new ConnectionFactory()
{
    Uri = new Uri("amqp://localhost")
};
using var connection = connectionFactory.CreateConnection();
using var channel = connection.CreateModel();

var requestQueueName = "example-request-response-queue";


channel.QueueDeclare(queue: requestQueueName, durable: false, exclusive: false, autoDelete: false);

var responseQueueName = channel.QueueDeclare().QueueName;

var correlationId = Guid.NewGuid().ToString();


#region Request message created

IBasicProperties properties = channel.CreateBasicProperties();
properties.CorrelationId = correlationId;
properties.ReplyTo = responseQueueName;


for (int i = 0; i < 100; i++)
{
    var message = Encoding.UTF8.GetBytes($"test : {i}");
    channel.BasicPublish(exchange: string.Empty, routingKey: requestQueueName, body: message,
        basicProperties: properties);
}

#endregion

#region Response Queue

EventingBasicConsumer consumer = new(channel);
channel.BasicConsume(queue: responseQueueName, autoAck: true, consumer: consumer);


consumer.Received += (sender, e) =>
{
    if (e.BasicProperties.CorrelationId == correlationId)
    {
        Console.WriteLine($"gelen mesaj : {Encoding.UTF8.GetString(e.Body.Span)}");
    }
};

#endregion