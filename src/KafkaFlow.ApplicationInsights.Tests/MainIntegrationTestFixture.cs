using System;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Testcontainers.Kafka;

namespace KafkaFlow.ApplicationInsights;


[TestFixture]
internal class MainIntegrationTestFixture
{
    private readonly KafkaContainer _kafkaContainer = new KafkaBuilder().Build();

    private WebApplicationFactory<Program> _factory;

    public static List<ITelemetry> dataStore = new();

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _kafkaContainer.StopAsync();
        await _kafkaContainer.DisposeAsync();
        await _factory.DisposeAsync();
    }

    [OneTimeSetUp]
    public async Task Startup()
    {

        await _kafkaContainer.StartAsync();
        var bootstrapServer = _kafkaContainer.GetBootstrapAddress();

        Environment.SetEnvironmentVariable("KAFKA_BROKERS", $"{bootstrapServer}");
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTests");
                builder.ConfigureServices(services =>
                {
                    //services.AddApplicationInsightsTelemetry("YOUR-KEY-FROM-ADP-MESSAGING-TEAM");
                    var sp = services.BuildServiceProvider();
                    var telemetryConfiguration = sp.GetService<TelemetryConfiguration>();
                    var tchan = new TestChannel(dataStore);
                    telemetryConfiguration.TelemetryChannel = tchan;
                    services.AddSingleton<ITelemetryChannel>(tchan);
                });
            });
    }

    [Test]
    public async Task WhenSendPostToHealthCheckEndpoint_ShouldReturnOkResponse()
    {
        var client = _factory.CreateClient();
        var result = await client.GetAsync("/");
        result.IsSuccessStatusCode.ShouldBeTrue();
        Thread.Sleep(5000);
        dataStore.Count.ShouldBe(7);
        dataStore.Where(y => y is DependencyTelemetry).Count(x => ((DependencyTelemetry)x).Type == "Kafka" && ((DependencyTelemetry)x).Name.StartsWith("Produce")).ShouldBe(3);
        dataStore.Where(y => y is DependencyTelemetry).Count(x => ((DependencyTelemetry)x).DependencyKind == "Kafka" && ((DependencyTelemetry)x).Name.StartsWith("Produce")).ShouldBe(3);
        dataStore.Where(y => y is DependencyTelemetry).Count(x => ((DependencyTelemetry)x).DependencyTypeName == "Kafka" && ((DependencyTelemetry)x).Name.StartsWith("Produce")).ShouldBe(3);
        dataStore.Where(y => y is DependencyTelemetry).Count(x => ((DependencyTelemetry)x).Type == "Kafka" && ((DependencyTelemetry)x).Name.StartsWith("Consume")).ShouldBe(3);
        dataStore.Where(y => y is DependencyTelemetry).Count(x => ((DependencyTelemetry)x).DependencyKind == "Kafka" && ((DependencyTelemetry)x).Name.StartsWith("Consume")).ShouldBe(3);
        dataStore.Where(y => y is DependencyTelemetry).Count(x => ((DependencyTelemetry)x).DependencyTypeName == "Kafka" && ((DependencyTelemetry)x).Name.StartsWith("Consume")).ShouldBe(3);
    }
}