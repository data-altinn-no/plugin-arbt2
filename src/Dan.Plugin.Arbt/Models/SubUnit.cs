using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dan.Plugin.Arbt.Models.Sub
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

        [JsonProperty("first")]
        public First First;

        [JsonProperty("prev")]
        public Prev Prev;

        [JsonProperty("next")]
        public Next Next;

        [JsonProperty("last")]
        public Last Last;
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

    public class Beliggenhetsadresse
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

    public class OverordnetEnhet
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Underenheter
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

        [JsonProperty("oppstartsdato")]
        public string Oppstartsdato;

        [JsonProperty("beliggenhetsadresse")]
        public Beliggenhetsadresse Beliggenhetsadresse;

        [JsonProperty("_links")]
        public Links Links;
    }

    public class Embedded
    {
        [JsonProperty("underenheter")]
        public List<Underenheter> Underenheter;
    }

    public class First
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Prev
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Next
    {
        [JsonProperty("href")]
        public string Href;
    }

    public class Last
    {
        [JsonProperty("href")]
        public string Href;
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

    public class SubUnit
    {
        [JsonProperty("_embedded")]
        public Embedded Embedded;

        [JsonProperty("_links")]
        public Links Links;

        [JsonProperty("page")]
        public Page Page;
    }


}
