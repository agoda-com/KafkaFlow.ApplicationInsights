using Microsoft.ApplicationInsights.Channel;

namespace KafkaFlow.ApplicationInsights;

public class TestChannel : ITelemetryChannel
{
    private readonly List<ITelemetry> _listTelemetries;

    public TestChannel(List<ITelemetry> listTelemetries)
    {
        _listTelemetries = listTelemetries;
    }

    public void Dispose()
    {

    }

    public void Send(ITelemetry item)
    {
        _listTelemetries.Add(item);
    }

    public void Flush()
    {

    }


    public bool? DeveloperMode { get; set; }
    public string EndpointAddress { get; set; }
}