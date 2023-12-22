using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using KafkaFlow;
using Microsoft.ApplicationInsights;

public class AppInsightsProducerEventsHandler
{
    internal static Task OnProducerStarted(IMessageContext eventContextMessageContext, TelemetryClient telemetryClient)
    {
        eventContextMessageContext.Items.Add("timer", Stopwatch.StartNew());
        eventContextMessageContext.Items.Add("telemetryClient", telemetryClient); 
        return Task.CompletedTask;
    }

    public static Task OnProducerError(IMessageContext eventContextMessageContext, Exception eventContextException)
    {
        eventContextMessageContext.Items.TryGetValue("telemetryClient", out var telemetryClientOut);
        var telemetryClient = (TelemetryClient)telemetryClientOut;
        telemetryClient.TrackException(eventContextException, new Dictionary<string, string>()
        {
            {"topic" , eventContextMessageContext.ProducerContext.Topic},
            {"partition" , eventContextMessageContext.ProducerContext.Partition.ToString()},
        });
        eventContextMessageContext.Items.TryGetValue("timer", out var timer);
        var theTimer = (Stopwatch)timer;
        theTimer.Stop();

        telemetryClient.TrackKafkaDependency(eventContextMessageContext, "Error", false, theTimer.Elapsed);
        return Task.CompletedTask;
    }

    public static Task OnProducerCompleted(IMessageContext eventContextMessageContext)
    {
        eventContextMessageContext.Items.TryGetValue("telemetryClient", out var telemetryClientOut);
        var telemetryClient = (TelemetryClient)telemetryClientOut;
        eventContextMessageContext.Items.TryGetValue("timer", out var timer);
        var theTimer = (Stopwatch)timer;
        theTimer.Stop();

        telemetryClient.TrackKafkaDependency(eventContextMessageContext, "Ok", true, theTimer.Elapsed);
        return Task.CompletedTask;
    }
}
