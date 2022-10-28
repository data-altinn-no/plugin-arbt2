using Newtonsoft.Json;
public class BemanningUnit
{
    [JsonProperty("Organisasjonsnummer")]
    public string Organisasjonsnummer { get; set; }

    [JsonProperty("Godkjenningsstatus")]
    public string Godkjenningsstatus { get; set; }
}