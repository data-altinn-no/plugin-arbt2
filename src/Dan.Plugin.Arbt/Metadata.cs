using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dan.Common;
using Dan.Common.Enums;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json.Schema.Generation;

namespace Dan.Plugin.Arbt;

/// <summary>
/// All plugins must implement IEvidenceSourceMetadata, which describes that datasets returned by this plugin. An example is implemented below.
/// </summary>
public class Metadata : IEvidenceSourceMetadata
{
    public const string SOURCE = "Arbeidstilsynet";
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<EvidenceCode> GetEvidenceCodes()
    {
        JSchemaGenerator generator = new JSchemaGenerator();

        return new List<EvidenceCode>()
        {
            new()
            {
                EvidenceCodeName = "Bemanningsforetakregisteret",
                EvidenceSource = SOURCE,
                Values = new List<EvidenceValue>()
                {
                    new()
                    {
                        EvidenceValueName = "Organisasjonsnummer",
                        ValueType = EvidenceValueType.String
                    },
                    new()
                    {
                        EvidenceValueName = "Godkjenningsstatus",
                        ValueType = EvidenceValueType.String
                    }
                }
            },
            new()
            {
                EvidenceCodeName = "Renholdsregisteret",
                EvidenceSource = SOURCE,
                Values = new List<EvidenceValue>()
                {
                        new()
                        {
                            EvidenceValueName = "Organisasjonsnummer",
                            ValueType = EvidenceValueType.String
                        },
                        new()
                        {
                            EvidenceValueName = "Status",
                            ValueType = EvidenceValueType.String
                        },
                        new()
                        {
                            EvidenceValueName = "StatusEndret",
                            ValueType = EvidenceValueType.DateTime
                        }
                }
            },
            new()
            {
                EvidenceCodeName = "Bilpleieregisteret",
                EvidenceSource = SOURCE,
                Values = new List<EvidenceValue>()
                {
                    new()
                    {
                        EvidenceValueName = "Organisasjonsnummer",
                        ValueType = EvidenceValueType.String
                    },
                    new()
                    {
                        EvidenceValueName = "Registerstatus",
                        ValueType = EvidenceValueType.Number
                    },
                    new()
                    {
                        EvidenceValueName = "RegisterstatusTekst",
                        ValueType = EvidenceValueType.String
                    },
                    new()
                    {
                        EvidenceValueName = "Godkjenningsstatus",
                        ValueType = EvidenceValueType.String
                    },
                    new()
                    {
                        EvidenceValueName = "Underenheter",
                        ValueType = EvidenceValueType.JsonSchema
                    }
                }
            }

        };
    }

    /// <summary>
    /// This function must be defined in all DAN plugins, and is used by core to enumerate the available datasets across all plugins.
    /// Normally this should not be changed.
    /// </summary>
    /// <param name="req"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function(Constants.EvidenceSourceMetadataFunctionName)]
    public async Task<HttpResponseData> GetMetadataAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
        FunctionContext context)
    {

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(GetEvidenceCodes());
        return response;
    }

}
