using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using KafkaFlow;
using Microsoft.ApplicationInsights;

public class AppInsightsConsumerEventsHandler
{
    public static Task OnConsumeStarted(IMessageContext eventContextMessageContext, TelemetryClient telemetryClient)
    {
        eventContextMessageContext.Items.Add("timer", Stopwatch.StartNew());
        eventContextMessageContext.Items.Add("telemetryClient", telemetryClient);
        return Task.CompletedTask;
    }

    public static Task OnConsumeError(IMessageContext eventContextMessageContext, Exception eventContextException,
        TelemetryClient telemetryClient)
    {
        telemetryClient.TrackException(eventContextException, new Dictionary<string, string>()
        {
            {"topic" , eventContextMessageContext.ConsumerContext.Topic},
            {"partition" , eventContextMessageContext.ConsumerContext.Partition.ToString()},
            {"offset" , eventContextMessageContext.ConsumerContext.Offset.ToString()},
        });
        eventContextMessageContext.Items.TryGetValue("timer", out var timer);
        var theTimer = (Stopwatch)timer;
        theTimer.Stop();
        telemetryClient.TrackDependency("Kafka-Consumer",
            eventContextMessageContext.ConsumerContext.Topic,
            eventContextMessageContext.ConsumerContext.Partition.ToString(),
            eventContextMessageContext.ConsumerContext.Offset.ToString(),
            DateTimeOffset.UtcNow,
            theTimer.Elapsed, "500", false);
        return Task.CompletedTask;
    }

    public static Task OnConsumeCompleted(IMessageContext eventContextMessageContext)
    {
        eventContextMessageContext.Items.TryGetValue("telemetryClient", out var telemetryClientOut);
        var telemetryClient = (TelemetryClient)telemetryClientOut;
        eventContextMessageContext.Items.TryGetValue("timer", out var timer);
        var theTimer = (Stopwatch)timer;
        theTimer.Stop();
        telemetryClient.TrackDependency("Kafka-Consumer",
            eventContextMessageContext.ConsumerContext.Topic,
            eventContextMessageContext.ConsumerContext.Partition.ToString(),
            eventContextMessageContext.ConsumerContext.Offset.ToString(),
            DateTimeOffset.UtcNow,
            theTimer.Elapsed, "200", true);
        return Task.CompletedTask;
    }
}