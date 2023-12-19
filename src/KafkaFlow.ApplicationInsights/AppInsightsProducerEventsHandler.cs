using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using KafkaFlow;
using Microsoft.ApplicationInsights;

public class AppInsightsProducerEventsHandler
{
    public static Task OnProducerStarted(IMessageContext eventContextMessageContext, TelemetryClient telemetryClient)
    {
        eventContextMessageContext.Items.Add("timer", Stopwatch.StartNew());
        return Task.CompletedTask;
    }

    public static Task OnProducerError(IMessageContext eventContextMessageContext, Exception eventContextException,
        TelemetryClient telemetryClient)
    {
        telemetryClient.TrackException(eventContextException, new Dictionary<string, string>()
        {
            {"topic" , eventContextMessageContext.ProducerContext.Topic},
            {"partition" , eventContextMessageContext.ProducerContext.Partition.ToString()},
            {"offset" , eventContextMessageContext.ProducerContext.Offset.ToString()},
        });
        eventContextMessageContext.Items.TryGetValue("timer", out var timer);
        var theTimer = (Stopwatch)timer;
        theTimer.Stop();
        var timeComponent = theTimer.ElapsedMilliseconds;
        telemetryClient.TrackDependency("Kafka-Producer",
            eventContextMessageContext.ProducerContext.Topic,
            eventContextMessageContext.ProducerContext.Partition.ToString(),
            eventContextMessageContext.ProducerContext.Offset.ToString(),
            DateTimeOffset.UtcNow,
            theTimer.Elapsed, "500", false);
        return Task.CompletedTask;
    }

    public static Task OnProducerCompleted(IMessageContext eventContextMessageContext, TelemetryClient telemetryClient)
    {
        eventContextMessageContext.Items.TryGetValue("timer", out var timer);
        var theTimer = (Stopwatch)timer;
        theTimer.Stop();
        var timeComponent = theTimer.ElapsedMilliseconds;
        telemetryClient.TrackDependency("Kafka-Producer", 
            eventContextMessageContext.ProducerContext.Topic, 
            eventContextMessageContext.ProducerContext.Partition.ToString(),
            eventContextMessageContext.ProducerContext.Offset.ToString(), 
            DateTimeOffset.UtcNow, 
            theTimer.Elapsed, "200", true);
        return Task.CompletedTask;
    }
}