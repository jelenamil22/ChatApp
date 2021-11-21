using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace ChatApp
{
    class ChatServer
    {
    }
    class Program1
    {
        static void Main1(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
akka {  
    actor {
        provider = remote
    }
    remote {
        dot-netty.tcp {
            port = 8081
            hostname = 0.0.0.0
            public-hostname = localhost
        }
    }
}
");

            using (var system = ActorSystem.Create("MyServer", config))
            {
                system.ActorOf(Props.Create(() => new ChatServerActor()), "ChatServer");

                Console.ReadLine();
            }
        }
    }

    class ChatServerActor : ReceiveActor, ILogReceive
    {
        private readonly HashSet<IActorRef> _clients = new HashSet<IActorRef>();

        public ChatServerActor()
        {
            Receive<SayRequest>(message =>
            {
                var response = new SayResponse
                {
                    Username = message.Username,
                    Text = message.Text,
                };
                foreach (var client in _clients) client.Tell(response, Self);
            });

            Receive<ConnectRequest>(message =>
            {
                _clients.Add(Sender);
                Sender.Tell(new ConnectResponse
                {
                    Message = "Hello and welcome to ChatApp",
                }, Self);
            });

            Receive<NickRequest>(message =>
            {
                var response = new NickResponse
                {
                    OldUsername = message.OldUsername,
                    NewUsername = message.NewUsername,
                };

                foreach (var client in _clients) client.Tell(response, Self);
            });
        }
    }
}
