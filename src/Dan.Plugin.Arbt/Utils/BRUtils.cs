using Dan.Plugin.Arbt.Models;
using Dan.Plugin.Arbt;
using Dan.Common.Exceptions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dan.Plugin.Arbt.Utils
{
    public class BRUtils
    {
        public static async Task<BREntityRegisterEntry> GetMainUnit(string organizationNumber, HttpClient client)
        {
            var org = await GetOrganizationInfoFromBR(organizationNumber, client);

            if (org != null && !string.IsNullOrEmpty(org.OverordnetEnhet))
                return await GetMainUnit(org.OverordnetEnhet, client);

            if (org == null)
            {
                throw new EvidenceSourcePermanentClientException(
                            Plugin.ERROR_ORGANIZATION_NOT_FOUND,
                            $"{organizationNumber} was not found in the Central Coordinating Register for Legal Entities");
            }

            return org;
        }

        public static async Task<BREntityRegisterEntry> GetOrganizationInfoFromBR(string organizationNumber, HttpClient client)
        {
            var rawResult = "";
            BREntityRegisterEntry result;

            try
            {
                var response = await client.GetAsync($"http://data.brreg.no/enhetsregisteret/api/enheter/{organizationNumber}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    response = await client.GetAsync(
                        $"http://data.brreg.no/enhetsregisteret/api/underenheter/{organizationNumber}");
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new EvidenceSourcePermanentClientException(
                            Plugin.ERROR_ORGANIZATION_NOT_FOUND,
                            $"{organizationNumber} was not found in the Central Coordinating Register for Legal Entities");
                    }
                }

                rawResult = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                throw new EvidenceSourcePermanentServerException(Plugin.ERROR_CCR_UPSTREAM_ERROR, null, e);
            }

            try
            {
                result = JsonConvert.DeserializeObject<BREntityRegisterEntry>(rawResult);
            }
            catch
            {
                throw new EvidenceSourcePermanentServerException(Plugin.ERROR_CCR_UPSTREAM_ERROR,
                    "Did not understand the data model returned from upstream source");
            }

            return result;
        }
    }
}
