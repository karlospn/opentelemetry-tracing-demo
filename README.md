# **Introduction**
This repository contains an example about how to use opentelemetry for tracing when we have a bunch of distributed applications

# **Content**

The repository contains the following applications:

![Alt Text](https://github.com/karlospn/opentelemetry-tracing-demo/blob/master/docs/components-diagram.png)

- **App1.WebApi** is a **NET 8 API** with 2 endpoints.
    - The **/http** endpoint makes an HTTP request to the App2 _"/dummy"_ endpoint.
    - The **/publish-message** endpoint queues a message into a Rabbit queue named _"sample"_.
    
- **App2.RabbitConsumer.Console** is a **NET 7 console** application. 
  - Dequeues messages from the Rabbit _"sample"_ queue and makes a HTTP request to the **App3** _"/sql-to-event"_ endpoint with the content of the message.

- **App3.WebApi** is a **NET 8 Minimal API** with 2 endpoints
    - The **/dummy** endpoint returns a fixed _"Ok"_ response.
    - The **/sql-to-event** endpoint receives a message via HTTP POST, stores it in a MSSQL Server and afterwards publishes the message as an event into a RabbitMq queue named _"sample_2"_.

- **App4.RabbitConsumer.HostedService** is a **NET 8 Worker Service**.
  - A Hosted Service reads the messages from the Rabbitmq _"sample_2"_ queue and stores it into a Redis cache database.

# **OpenTelemetry .NET Client**

The apps are using the following package versions:

```xml
  <PackageReference Include="OpenTelemetry" Version="1.8.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.8.1" />
  <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.8.0" />
  <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.8.0" />
  <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.8.1" />
  <PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.8.0-beta.1" />
  <PackageReference Include="ZiggyCreatures.FusionCache.OpenTelemetry" Version="1.0.0" />
```

# **External Dependencies**

- Jaeger 
- MSSQL Server
- RabbitMq
- Redis Cache


# **How to run the apps**

##  **Using docker-compose**

The repository contains  a **docker-compose** file that starts up the 4 apps and also the external dependencies.   
There is a **little caveat in the docker-compose**: 
- You can control the order of service startup and shutdown with the depends_on option. However, for startup Compose does not wait until a container is “ready” only until it’s running.    
That's a problem because both App3 and App4 need to wait for the rabbitMq container to be ready. To avoid this problem the docker-compose is overwriting the "entrypoint" for both apps and executing a shell script that makes both apps sleep 30 seconds before starting up.

## **Without using the docker-compose**

If you **don't want to use the docker-compose file**, you can use docker to start the dependencies one by one.

- Run a Jaeger image:
```shell
docker run -d --name jaeger \
  -e COLLECTOR_ZIPKIN_HOST_PORT=:9411 \
  -e COLLECTOR_OTLP_ENABLED=true \
  -p 6831:6831/udp \
  -p 6832:6832/udp \
  -p 5778:5778 \
  -p 16686:16686 \
  -p 4317:4317 \
  -p 4318:4318 \
  -p 14250:14250 \
  -p 14268:14268 \
  -p 14269:14269 \
  -p 9411:9411 \
  jaegertracing/all-in-one:latest
```
- Run a Rabbitmq image:

```shell
docker run -d --name some-rabbit \
  -p 15672:15672 \
  -p 5672:5672 \
  rabbitmq:3.13.1-management
```
- Run a MSSQL Server

```shell
docker run -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=Pass@Word1" \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2019-GA-ubuntu-16.04
```

- Run a Redis immage:

```shell
docker run -d --name some-redis \
  -p "6379:6379" \
  redis:7.2.4
```

# Output

If you open Jaeger, you are going to see something like this

![Alt Text](https://github.com/karlospn/opentelemetry-tracing-demo/blob/master/docs/jaeger.png)

# Changelog

### **04/15/2024**
- Updated apps to .NET 8.
- Updated ``OpenTelemetry`` packages to the latest available version. This update also addresses several known security vulnerabilities.
- Updated ``System.Data.SqlClient`` to the latest available version to mitigate known security vulnerabilities.
- From this point forward, ``App 1`` will function as a .NET 8 API that utilizes Controllers, while ``App 3`` will operate as a minimal API without controllers.
- Deleted ``Startup.cs`` from both App 1 and App 3.
- ``App 1`` and ``App 3`` now employ the newer ``WebApplication.CreateBuilder`` method for application construction, replacing the previous ``WebHost.CreateDefaultBuilder`` method.
- ``App 4`` now utilizes the newer ``Host.CreateApplicationBuilder`` method to construct a ``Host``, replacing the previous ``Host.CreateDefaultBuilder`` method.
- Implemented the C# 12 primary constructor feature across all apps.
- Updated Dockerfile base image from ``Bullseye`` distro (Debian 11) to ``Bookworm`` distro (Debian 12).
- ``App 4`` now employs the [FusionCache](https://github.com/ZiggyCreatures/FusionCache/tree/main) library with a Redis Backplane, replacing the use of ``IDistributedCache``.
- Updated the ``docker-compose`` file to utilize the latest image versions of RabbitMQ, Redis, and Jaeger.


### **09/24/2023**
- Updated apps to .NET 7.
- Updated ``OpenTelemetry`` packages to the latest version.
- Fix breaking changes on the apps due to the ``OpenTelemetry`` packages upgrade.
- Removed the ``OpenTelemetry.Exporter.Jaeger`` NuGet package from the apps because it has been deprecated. It has been replaced by the ``OpenTelemetry.Exporter.OpenTelemetryProtocol`` package.
- Updated the ``RabbitMQ.Client`` NuGet package to the latest version.
- Updated the ``docker-compose`` file to use the newest image versions of rabbitmq, redis and jaeger. Also the jaeger image is configured so can it can receive OpenTelemetry trace data via the OpenTelemetry Protocol.