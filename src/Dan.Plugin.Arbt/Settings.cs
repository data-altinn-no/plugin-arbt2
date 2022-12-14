namespace Dan.Plugin.Arbt;

public class Settings
{
    public int DefaultCircuitBreakerOpenCircuitTimeSeconds { get; set; }
    public int DefaultCircuitBreakerFailureBeforeTripping { get; set; }
    public int SafeHttpClientTimeout { get; set; }

    public string EndpointUrl { get; set; }

    public string BemanningUrl { get; set; }
    public string RenholdUrl { get; set; }
    public string BilpleieUrl { get; set; }
}
