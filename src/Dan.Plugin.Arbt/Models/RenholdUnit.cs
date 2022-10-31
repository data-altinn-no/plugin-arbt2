using Newtonsoft.Json;
using System;
public class RenholdUnit
{
    [JsonProperty("StatusEndret")]
    public DateTime StatusEndret { get; set; }

    [JsonProperty("Organisasjonsnummer")]
    public string Organisasjonsnummer { get; set; }

    [JsonProperty("Status")]
    public string Status { get; set; }
}