using System;
using System.IO;
using System.Collections;
using System.Data;
//using sctt.utility;
using System.Collections.Generic;
//using ItemTracking.Models.SCTT.Item;
using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SPT
{
    /// <summary>
    /// Summary description for ITS_Log
    /// </summary>
    public class RabbitMQProducer
    {

        public static void SendToRabbitMQ(string queueName, string message, IConfiguration env)
        {
            try
            {
                var factory = new ConnectionFactory()
                {                      
                    UserName = env.GetSection("RABBITMQ_UserName").Value,
                    Password = env.GetSection("RABBITMQ_Password").Value,
                    HostName = env.GetSection("RABBITMQ_HostName").Value,
                    //VirtualHost = env.GetSection("RABBITMQHOST").Value,
                    Port = Convert.ToInt32(env.GetSection("RABBITMQ_PORT").Value)
                };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    //string message = greeting.Greet;
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




    }
}