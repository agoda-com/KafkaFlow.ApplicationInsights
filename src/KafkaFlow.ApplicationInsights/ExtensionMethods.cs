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

            hub.MessageConsumeError.Subscribe(eventContext => AppInsightsConsumerEventsHandler.OnConsumeError(eventContext.MessageContext, eventContext.Exception, eventContext.MessageContext.DependencyResolver.Resolve<TelemetryClient>()));

            hub.MessageConsumeCompleted.Subscribe(eventContext => AppInsightsConsumerEventsHandler.OnConsumeCompleted(eventContext.MessageContext));

            hub.MessageProduceStarted.Subscribe(eventContext => AppInsightsProducerEventsHandler.OnProducerStarted(eventContext.MessageContext, eventContext.MessageContext.DependencyResolver.Resolve<TelemetryClient>()));

            hub.MessageProduceError.Subscribe(eventContext => AppInsightsProducerEventsHandler.OnProducerError(eventContext.MessageContext, eventContext.Exception, eventContext.MessageContext.DependencyResolver.Resolve<TelemetryClient>()));

            hub.MessageProduceCompleted.Subscribe(eventContext => AppInsightsProducerEventsHandler.OnProducerCompleted(eventContext.MessageContext, eventContext.MessageContext.DependencyResolver.Resolve<TelemetryClient>()));
        });

        return builder;
    }
}