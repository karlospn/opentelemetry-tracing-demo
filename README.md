# Introduction
These repository contains an example about how to use opentelemetry for tracing when we have a bunch of distributed applications

# Content

The repository contains the following applications:

![Alt Text](https://github.com/karlospn/opentelemetry-tracing-demo/blob/master/docs/diagram.jpg)


- **App1.WebApi** is a .NET Core 3.1 WebApi with 2 endpoints
    - /http/app2 : makes a http call to App2 "dummy" endpoint
    - /rabbit/app3 : publishes a message inside a RabbitMq queue named "sample"
    
- **App2.WebApi** is a .NET Core 3.1 WebApi with 2 endpoints
    - /dummy : returns a "dummy" string
    - /sql/save : receives a message and stores it inside an SQL. After the message is stored, it publishes a MessagePersisted event into a RabbitMq exchange named "MessagePersistedEvent".

- **App3.RabbitConsumer.ConsoleApp** is a .NET Core 3.1 console application. It reads the messages from Rabbitmq "sample" queue and makes and Http call to App2.WebApi "/sql/save" endpoint with the content of the message.

- **App4.RabbitConsumer.HostedService** is a .NET Core 3.1 console application with a HostedService. First of all it creates a rabbitMq queue named "MessagePersistedEvent" and binds it with the "MessagePersistentEvent" exchange. After that it processes the messages from the queue.

    
# Requirements

//TODO
