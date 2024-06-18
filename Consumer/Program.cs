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


EventingBasicConsumer consumer = new(channel);


channel.BasicConsume(queue: requestQueueName, autoAck: true, consumer: consumer);


consumer.Received += (sender, eventArgs) =>
{
    string message = Encoding.UTF8.GetString(eventArgs.Body.Span);
    Console.WriteLine(message);

    var responseMessage = Encoding.UTF8.GetBytes($"İşlem tamamlandı.:{message}");

    IBasicProperties basicProperties = channel.CreateBasicProperties();
    basicProperties.CorrelationId = eventArgs.BasicProperties.CorrelationId;

    channel.BasicPublish(exchange: string.Empty, routingKey: eventArgs.BasicProperties.ReplyTo,
        basicProperties: basicProperties, body: responseMessage);
};