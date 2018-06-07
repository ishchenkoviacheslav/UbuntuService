using DTO;
using Model;
using MyLogger;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serializator;
using Sharing.DTO;
using System;
using System.Collections.Generic;
using System.IO;

namespace UbuntuService
{
    class MasterClient: IDisposable
    {
        Logger logger = new Logger();
        private List<ClientPeer> ClientsList = new List<ClientPeer>();
        public IConnection connection;
        IModel channel;
        public MasterClient()
        {
            logger.Info("Server started");
            ConnectionFactory factory = new ConnectionFactory() { UserName = "user", Password = "user", HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue: "MasterClient", durable: false, exclusive: false, autoDelete: false, arguments: null);
            //channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: "MasterClient", autoAck: true, consumer: consumer);
            consumer.Received += (model, ea) =>
            {
                IBasicProperties props = ea.BasicProperties;
                if (!ClientsList.Exists((c) => c.QeueuName == props.ReplyTo))
                {
                    ClientsList.Add(new ClientPeer() { QeueuName = props.ReplyTo, LastUpTime = DateTime.UtcNow });
                    logger.Info($"Was added a client: {props.ReplyTo}");
                }
                Object obtained = ea.Body.Serializer();
                switch (obtained)
                {
                    case JsonClass j:
                        logger.Info("JsonClass");
                        JsonModel jsonModel = new JsonModel() { IP = "localhost", UserName = "user", Password = "123456" };
                        string jsonString = JsonConvert.SerializeObject(jsonModel, Formatting.Indented);
                        JsonClass jsonClass = new JsonClass()
                        {
                            JsonByteArray = jsonString.Serializer()
                        };
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: null, body: jsonClass.Serializer());

                        break;
                    case ImageClass i:
                        logger.Info("ImageClass");
                        ImageClass image = new ImageClass()
                        {
                            ImageByteArray = GetImage()
                        };
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: null, body: image.Serializer());
                        break;
                    case FileClass f:
                        logger.Info("FileClass");
                        FileClass file = new FileClass()
                        {
                            FileByteArray = GetFile()
                        };
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: null, body: file.Serializer());
                        break;
                    default:
                        logger.Error("Type is different!");
                        break;
                }//switch
            };
        }//ctor

        public byte[] GetImage(string path = @"/home/viacheslav/Documents/image.jpg")
        {
            using (Stream fs = File.OpenRead(path))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fs.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            
        }

        public byte[] GetFile(string path = @"/home/viacheslav/Documents/file.dat")
        {
            using (Stream fs = File.OpenRead(path))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fs.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public void Dispose()
        {
            channel.Close();
            connection.Close();
            logger.Info("Server stoped");
        }
    }
}
