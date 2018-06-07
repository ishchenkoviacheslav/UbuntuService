using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serializator;
using Sharing.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace API
{
    public class ClientsAPI: IDisposable
    {
        private IConnection connection;
        private IModel channel;
        private readonly string replyQueueName;
        private EventingBasicConsumer consumer;
        private IBasicProperties props;

        event Action<string> JsonResponse;
        event Action<byte[]> ImageResponse;
        event Action<byte[]> FileResponse;
        public ClientsAPI()
        {
            ConnectionFactory factory = new ConnectionFactory() { UserName = "user", Password = "user", HostName = "" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);
            consumer.Received += (model, ea) =>
            {
                Object obtained = ea.Body.Deserializer();
                switch (obtained)
                {
                    case JsonClass j:
                        JsonResponse(System.Text.Encoding.Default.GetString(j.JsonByteArray));
                        break;
                    case ImageClass i:
                        ImageResponse(i.ImageByteArray);
                        break;
                    case FileClass f:
                        FileResponse(f.FileByteArray);
                        break;
                    default:
                        throw new Exception("Type if different! Server sent unknown type!");
                }
            };
        }

        public void GiveMeJson()
        {
            JsonClass jsonData = new JsonClass();
            channel.BasicPublish(exchange: "", routingKey: "MasterClient", basicProperties: props, body: jsonData.Serializer());
        }
        public void GiveMeImage()
        {
            ImageClass image = new ImageClass();
            channel.BasicPublish(exchange: "", routingKey: "MasterClient", basicProperties: props, body: image.Serializer());
        }
        public void GiveMeFile()
        {
            FileClass file = new FileClass();
            channel.BasicPublish(exchange: "", routingKey: "MasterClient", basicProperties: props, body: file.Serializer());
        }
        public void Dispose()
        {
            connection.Close();
        }
    }
}
