# ApplicationInsights for KafkaFlow 

Exentation of [KafkaFlow](https://github.com/Farfetch/kafkaflow) that allows for telemetry data to be sent to applcaition insights.

## Usage

Install the package to existin project using KafkaFlow

```powershell
dotnet add package KafkaFlow.ApplicationInsights
```

Add code to startup/program.cs

```csharp

builder.Services.AddKafka(
    kafka => kafka
        .AddAppInsightsInstrumentation() // <-- add this line
        .AddCluster(
            cluster => cluster
                .WithBrokers(new[] { broker })
                .AddProducer(
                    // producer
                )
                .AddConsumer(consumer => consumer
                    // consumer
                )
        )
);

```
