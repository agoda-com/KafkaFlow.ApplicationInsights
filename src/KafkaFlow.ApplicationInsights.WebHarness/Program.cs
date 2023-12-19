using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("KafkaFlow.ApplicationInsights.Tests")]

var builder = WebApplication.CreateBuilder(args);

const string topicName = "sample-topic";
const string producerName = "say-hello";
var broker = Environment.GetEnvironmentVariable("KAFKA_BROKERS") ?? "localhost:9092";
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddKafka(
    kafka => kafka
        .UseConsoleLog()
        .AddAppInsightsInstrumentation()
        .AddCluster(
            cluster => cluster
                .WithBrokers(new[] { broker })
                .CreateTopicIfNotExists(topicName, 1, 1)
                .WithSecurityInformation(x =>
                {
                    x.EnableSslCertificateVerification = false;
                    x.SecurityProtocol = SecurityProtocol.Plaintext;
                })
                .AddProducer(
                    producerName,
                    producer => producer
                        .DefaultTopic(topicName)
                        .AddMiddlewares(m =>
                            m.AddSerializer<JsonCoreSerializer>()
                        )
                )
                .AddConsumer(consumer => consumer
                    .Topic(topicName)
                    .WithGroupId("sample-group")
                    .WithBufferSize(1)
                    .WithWorkersCount(1)
                    .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h.AddHandler<HelloMessageHandler>())
                    )
                )
        )
);


var app = builder.Build();

var bus = app.Services.CreateKafkaBus();
await bus.StartAsync();

app.MapGet("/", async (IProducerAccessor producerAccessor) =>
{

    var topicName = "sample-topic";
    var producer = producerAccessor.GetProducer(producerName);

    await producer.ProduceAsync(
        topicName,
        Guid.NewGuid().ToString(),
        new HelloMessage { Text = "1111Hello!" });

    await producer.ProduceAsync(
        topicName,
        Guid.NewGuid().ToString(),
        new HelloMessage { Text = "2222Hello!" });

    await producer.ProduceAsync(
        topicName,
        Guid.NewGuid().ToString(),
        new HelloMessage { Text = "3333Hello!" });

    return "Hello World!";

});


app.Run();
