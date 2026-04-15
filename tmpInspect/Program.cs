using System;
using RabbitMQ.Client;

class Program {
  static void Main() {
    Console.WriteLine(typeof(ConnectionFactory).GetMethod("CreateConnection").ToString());
    Console.WriteLine(typeof(IConnection).GetMethod("CreateModel").ToString());
    Console.WriteLine(typeof(RabbitMQ.Client.Events.EventingBasicConsumer).FullName);
  }
}
