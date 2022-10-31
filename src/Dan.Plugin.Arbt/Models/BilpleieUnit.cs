using Newtonsoft.Json;
public class BilpleieUnit
{
    [JsonProperty("metadata")]
    public BilpleieMetadata Metadata { get; set; }

    [JsonProperty("data")]
    public BilpleieData Data { get; set; }

}

public class BilpleieMetadata
{
    [JsonProperty("versjon")]
    public string Versjon;

    [JsonProperty("datoTidGenerert")]
    public string DatoTidGenerert;
}

public class BilpleieData
{
    [JsonProperty("organisasjonsnummer")]
    public string Organisasjonsnummer { get; set; }

    [JsonProperty("registerstatus")]
    public int Registerstatus { get; set; }

    [JsonProperty("registerstatusTekst")]
    public string RegisterstatusTekst { get; set; }

    [JsonProperty("godkjenningsstatus")]
    public string Godkjenningsstatus { get; set; }
}