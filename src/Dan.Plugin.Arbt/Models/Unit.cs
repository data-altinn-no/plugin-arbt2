using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dan.Plugin.Arbt.Models.Unit
{
    public class Self
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Links
    {
        [JsonProperty("self")]
        public Self Self;

        [JsonProperty("overordnetEnhet")]
        public OverordnetEnhet OverordnetEnhet;
    }

    public class Organisasjonsform
    {
        [JsonProperty("kode")]
        public string Kode;

        [JsonProperty("beskrivelse")]
        public string Beskrivelse;

        [JsonProperty("_links")]
        public Links Links;
    }

    public class Postadresse
    {
        [JsonProperty("land")]
        public string Land;

        [JsonProperty("landkode")]
        public string Landkode;

        [JsonProperty("postnummer")]
        public string Postnummer;

        [JsonProperty("poststed")]
        public string Poststed;

        [JsonProperty("adresse")]
        public List<string> Adresse;

        [JsonProperty("kommune")]
        public string Kommune;

        [JsonProperty("kommunenummer")]
        public string Kommunenummer;
    }

    public class Naeringskode1
    {
        [JsonProperty("beskrivelse")]
        public string Beskrivelse;

        [JsonProperty("kode")]
        public string Kode;
    }

    public class Forretningsadresse
    {
        [JsonProperty("land")]
        public string Land;

        [JsonProperty("landkode")]
        public string Landkode;

        [JsonProperty("postnummer")]
        public string Postnummer;

        [JsonProperty("poststed")]
        public string Poststed;

        [JsonProperty("adresse")]
        public List<string> Adresse;

        [JsonProperty("kommune")]
        public string Kommune;

        [JsonProperty("kommunenummer")]
        public string Kommunenummer;
    }

    public class InstitusjonellSektorkode
    {
        [JsonProperty("kode")]
        public string Kode;

        [JsonProperty("beskrivelse")]
        public string Beskrivelse;
    }

    public class OverordnetEnhet
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Naeringskode2
    {
        [JsonProperty("beskrivelse")]
        public string Beskrivelse;

        [JsonProperty("kode")]
        public string Kode;
    }

    public class Enheter
    {
        [JsonProperty("organisasjonsnummer")]
        public string Organisasjonsnummer;

        [JsonProperty("navn")]
        public string Navn;

        [JsonProperty("organisasjonsform")]
        public Organisasjonsform Organisasjonsform;

        [JsonProperty("postadresse")]
        public Postadresse Postadresse;

        [JsonProperty("registreringsdatoEnhetsregisteret")]
        public string RegistreringsdatoEnhetsregisteret;

        [JsonProperty("registrertIMvaregisteret")]
        public bool RegistrertIMvaregisteret;

        [JsonProperty("naeringskode1")]
        public Naeringskode1 Naeringskode1;

        [JsonProperty("antallAnsatte")]
        public int AntallAnsatte;

        [JsonProperty("overordnetEnhet")]
        public string OverordnetEnhet;

        [JsonProperty("forretningsadresse")]
        public Forretningsadresse Forretningsadresse;

        [JsonProperty("stiftelsesdato")]
        public string Stiftelsesdato;

        [JsonProperty("institusjonellSektorkode")]
        public InstitusjonellSektorkode InstitusjonellSektorkode;

        [JsonProperty("registrertIForetaksregisteret")]
        public bool RegistrertIForetaksregisteret;

        [JsonProperty("registrertIStiftelsesregisteret")]
        public bool RegistrertIStiftelsesregisteret;

        [JsonProperty("registrertIFrivillighetsregisteret")]
        public bool RegistrertIFrivillighetsregisteret;

        [JsonProperty("konkurs")]
        public bool Konkurs;

        [JsonProperty("underAvvikling")]
        public bool UnderAvvikling;

        [JsonProperty("underTvangsavviklingEllerTvangsopplosning")]
        public bool UnderTvangsavviklingEllerTvangsopplosning;

        [JsonProperty("maalform")]
        public string Maalform;

        [JsonProperty("_links")]
        public Links Links;

        [JsonProperty("hjemmeside")]
        public string Hjemmeside;

        [JsonProperty("naeringskode2")]
        public Naeringskode2 Naeringskode2;

        [JsonProperty("sisteInnsendteAarsregnskap")]
        public string SisteInnsendteAarsregnskap;
    }

    public class Embedded
    {
        [JsonProperty("enheter")]
        public List<Enheter> Enheter;
    }

    public class Page
    {
        [JsonProperty("size")]
        public int Size;

        [JsonProperty("totalElements")]
        public int TotalElements;

        [JsonProperty("totalPages")]
        public int TotalPages;

        [JsonProperty("number")]
        public int Number;
    }

    public class Unit
    {
        [JsonProperty("_embedded")]
        public Embedded Embedded;

        [JsonProperty("_links")]
        public Links Links;

        [JsonProperty("page")]
        public Page Page;
    }


    public class BilpleieResponse
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

        [JsonProperty("underenheter")]
        public List<Enheter> Underenheter { get; set; }
    }

    public class BemanningUnit
    {
        [JsonProperty("Organisasjonsnummer")]
        public string Organisasjonsnummer { get; set; }

        [JsonProperty("Godkjenningsstatus")]
        public string Godkjenningsstatus { get; set; }
    }

    public class RenholdUnit
    {
        [JsonProperty("StatusEndret")]
        public DateTime StatusEndret { get; set; }

        [JsonProperty("Organisasjonsnummer")]
        public string Organisasjonsnummer { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }
    }
}
