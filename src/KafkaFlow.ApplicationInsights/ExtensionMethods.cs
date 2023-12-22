using System;
using KafkaFlow;
using KafkaFlow.Configuration;
using Microsoft.ApplicationInsights;

/// <summary>
/// Adds AppInsights instrumentation
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Adds AppInsights instrumentation
    /// </summary>
    /// <param name="builder">The Kafka configuration builder</param>
    /// <returns></returns>
    public static IKafkaConfigurationBuilder AddAppInsightsInstrumentation(this IKafkaConfigurationBuilder builder)
    {
        builder.SubscribeGlobalEvents(hub =>
        {
            hub.MessageConsumeStarted.Subscribe(eventContext => AppInsightsConsumerEventsHandler.OnConsumeStarted(eventContext.MessageContext, eventContext.MessageContext.DependencyResolver.Resolve<TelemetryClient>()));

            hub.MessageConsumeError.Subscribe(eventContext => AppInsightsConsumerEventsHandler.OnConsumeError(eventContext.MessageContext, eventContext.Exception));

            hub.MessageConsumeCompleted.Subscribe(eventContext => AppInsightsConsumerEventsHandler.OnConsumeCompleted(eventContext.MessageContext));

            hub.MessageProduceStarted.Subscribe(eventContext => AppInsightsProducerEventsHandler.OnProducerStarted(eventContext.MessageContext, eventContext.MessageContext.DependencyResolver.Resolve<TelemetryClient>()));

            hub.MessageProduceError.Subscribe(eventContext => AppInsightsProducerEventsHandler.OnProducerError(eventContext.MessageContext, eventContext.Exception));

            hub.MessageProduceCompleted.Subscribe(eventContext => AppInsightsProducerEventsHandler.OnProducerCompleted(eventContext.MessageContext));
        });

        return builder;
    }

    internal static string GetDependencyName(this IMessageContext eventContextMessageContext)
    {
        return eventContextMessageContext.ConsumerContext?.Topic ??
               eventContextMessageContext.ProducerContext?.Topic;
    }

    internal static string GetData(this IMessageContext eventContextMessageContext)
    {
        var type = Constants.ConsumerType;
        if (eventContextMessageContext.ConsumerContext == null)
        {
            type = Constants.ProducerType;
        }

        var partition = eventContextMessageContext.ConsumerContext?.Partition.ToString() ??
                        eventContextMessageContext.ProducerContext?.Partition.ToString();

        return $"{type}/{partition}";
    }

    internal static string GetTarget(this IMessageContext eventContextMessageContext)
    {
        return string.Join(",", eventContextMessageContext.Brokers);
    }

    internal static void TrackKafkaDependency(this TelemetryClient telemetryClient, IMessageContext eventContextMessageContext, string resultCode,
        bool success, TimeSpan elapsedTime)
    {
        telemetryClient.TrackDependency(Constants.DependencyType,
                                    eventContextMessageContext.GetTarget(),
                                    eventContextMessageContext.GetDependencyName(),
                                      eventContextMessageContext.GetData(),
                                      DateTimeOffset.UtcNow,
                                      elapsedTime,
                                      resultCode,
                                      success);
    }
}

