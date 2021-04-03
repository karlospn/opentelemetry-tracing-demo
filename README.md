# Introduction
These repository contains an example about how to use opentelemetry for tracing when we have a bunch of distributed applications

# Content

The repository contains the following applications:

![Alt Text](https://github.com/karlospn/opentelemetry-tracing-demo/blob/master/docs/components-diagram.png)


- **App1.WebApi** is a .NET5 WebApi with 2 endpoints
    - /http : makes a http call to App2 "dummy" endpoint
    - /publish-message : publishes a message inside a RabbitMq queue named "sample"
    
- **App2.WebApi** is a .NET5 WebApi with 2 endpoints
    - /dummy : returns a dummy "Ok" string
    - /sql-to-event : receives a message and stores it inside an SQL. After the message is stored, it publishes a MessagePersisted event into a RabbitMq exchange named "MessagePersistedEvent".

- **App3.RabbitConsumer.Console** is a .NET5 console application. It reads the messages from Rabbitmq "sample" queue and makes and Http call to App2.WebApi "/sql-to-event" endpoint with the content of the message.

- **App4.RabbitConsumer.HostedService** is a .NET5 console application with a HostedService. It reads the messages from Rabbitmq "sample_2" queue and writes the result in the console

    
# Requirements

- Jaeger 
- SQL Server
- RabbitMq
- App1 must run on port 5000
- App2 must run on port 5001

You can use docker:

- docker run -d --name jaeger -e COLLECTOR_ZIPKIN_HTTP_PORT=19411 -p 5775:5775/udp -p 6831:6831/udp  -p 6832:6832/udp  -p 5778:5778   -p 16686:16686  -p 14268:14268  -p 19411:19411   jaegertracing/all-in-one
- docker run -d --hostname my-rabbit --name some-rabbit -p 8082:15672 -p 5672:5672 rabbitmq:3.6.15-management
- docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@Word1" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-GA-ubuntu-16.04

Or use the docker-compose in the repository

# Output

If you open jaeger you are going to see something like this

![Alt Text](https://github.com/karlospn/opentelemetry-tracing-demo/blob/master/docs/jaeger.png)
