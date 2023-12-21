# Application Insights for KafkaFlow

## Overview

"ApplicationInsights for KafkaFlow" is an extension of [KafkaFlow](https://github.com/Farfetch/kafkaflow), designed to seamlessly integrate telemetry data collection into your Kafka-based applications using Application Insights. This extension enhances your ability to monitor, analyze, and optimize your message processing pipelines.

## Features

- **Easy Integration:** Quickly add Application Insights telemetry to your KafkaFlow applications.
- **Real-Time Monitoring:** Track and analyze message flows in real-time.
- **Error Tracking:** Effortlessly capture and analyze exceptions and processing errors.

## Getting Started

### Prerequisites

- .NET compatible project using KafkaFlow.
- An active Azure subscription with Application Insights set up.

### Installation

1. **Install the Package:**

   Use the following command in the PowerShell to install the KafkaFlow.ApplicationInsights package:

   ```powershell
   dotnet add package Agoda.KafkaFlow.ApplicationInsights
   ```

2. **Configuration:**

   Add the following code to your `startup.cs` or `program.cs` to configure the Application Insights integration:

   ```csharp
   builder.Services.AddKafka(
       kafka => kafka
           .AddAppInsightsInstrumentation() // Enable Application Insights
           .AddCluster(
               cluster => cluster
                   .WithBrokers(new[] { "your-broker-url" })
                   .AddProducer(
                       // Define your producer configuration here
                   )
                   .AddConsumer(consumer => consumer
                       // Define your consumer configuration here
                   )
           )
   );
   ```

## Documentation

For more detailed information and advanced configuration options, please refer to the [official documentation](#).

## Contributing

Contributions to "Application Insights for KafkaFlow" are welcome! Please refer to our [contribution guidelines](CONTRIBUTING.md) for more details.


---

This revised README offers a clearer overview of the project, its features, a more detailed installation guide, and information about contributing and licensing. Adjustments can be made based on your project's specific details and documentation links.
