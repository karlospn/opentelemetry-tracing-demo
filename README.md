# Introduction
This repository contains an example about how to use opentelemetry for tracing when we have a bunch of distributed applications

# Content

The repository contains the following applications:

![Alt Text](https://github.com/karlospn/opentelemetry-tracing-demo/blob/master/docs/components-diagram.png)


- **App1.WebApi** is a .NET6 WebApi with 2 endpoints:
    - **/http** endpoint : makes a http call to App2 "dummy" endpoint
    - **/publish-message** endpoint : publishes a message into a RabbitMq queue named _"sample"_
    
- **App2.WebApi** is a .NET6 WebApi with 2 endpoints
    - **/dummy** endpoint : returns a fixed "Ok" response.
    - **/sql-to-event** endpoint : receives a message and stores it inside an SQL Server. After the message is stored, it publishes an event into a RabbitMq queue named _"sample_2"_.

- **App3.RabbitConsumer.Console** is a .NET6 console application. 
  - Reads the messages from the Rabbitmq _"sample"_ queue and makes and Http call to **App2.WebApi** _"/sql-to-event"_ endpoint with the content of the message.

- **App4.RabbitConsumer.HostedService** is a .NET6 Worker Service.
  - The Hosted Service reads the messages from the Rabbitmq _"sample_2"_ queue and stores it into a Redis cache database.


# OpenTelemetry .NET Client

The apps are using the following OpenTelemetry versions

```xml
<PackageReference Include="OpenTelemetry" Version="1.2.0-rc1" />
<PackageReference Include="OpenTelemetry.Exporter.Jaeger" Version="1.2.0-rc1" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.0.0-rc8" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.0.0-rc8" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.0.0-rc8" />
<PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.0.0-rc8" />
<PackageReference Include="OpenTelemetry.Instrumentation.StackExchangeRedis" Version="1.0.0-rc8" />
```

# External Dependencies

- Jaeger 
- MSSQL Server
- RabbitMq
- Redis Cache


# How to run the apps

The repository contains  a **docker-compose** file that starts up the 4 apps and also the external dependencies.   
There is a **little caveat in the docker-compose**: 
- You can control the order of service startup and shutdown with the depends_on option. However, for startup Compose does not wait until a container is “ready” only until it’s running.    
That's a problem because both App3 and App4 need to wait for the rabbitMq container to be ready. To avoid this problem the docker-compose is overwriting the "entrypoint" for both apps and executing a shell script that makes both apps sleep 30 seconds before starting up.


If you don't want to use the compose file you can use docker to start the dependencies manually, you can ran the following commands:

- _docker run -d --name jaeger -e COLLECTOR_ZIPKIN_HTTP_PORT=19411 -p 5775:5775/udp -p 6831:6831/udp  -p 6832:6832/udp  -p 5778:5778   -p 16686:16686  -p 14268:14268  -p 19411:19411   jaegertracing/all-in-one_
- _docker run -d --hostname my-rabbit --name some-rabbit -p 8082:15672 -p 5672:5672 rabbitmq:3.6.15-management_
- _docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@Word1" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-GA-ubuntu-16.04_
- _docker run -d --name some-redis -p "6379:6379" redis:6.2.1_


# Output

If you open jaeger you are going to see something like this

![Alt Text](https://github.com/karlospn/opentelemetry-tracing-demo/blob/master/docs/jaeger.png)
