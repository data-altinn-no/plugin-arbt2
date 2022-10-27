using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dan.Plugin.Arbt.Utils;
using Dan.Common;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BilpleieUnit = Dan.Plugin.Arbt.Models.Unit.BilpleieResponse;
using BemanningUnit = Dan.Plugin.Arbt.Models.Unit.BemanningUnit;
using RenholdUnit = Dan.Plugin.Arbt.Models.Unit.RenholdUnit;

namespace Dan.Plugin.Arbt;
public class Plugin
{
    private ILogger _logger;
    private readonly HttpClient _client;
    private readonly Settings _settings;
    private readonly IEvidenceSourceMetadata _metadata;
    // The datasets must supply a human-readable source description from which they originate. Individual fields might come from different sources, and this string should reflect that (ie. name all possible sources).

    // These are not mandatory, but there should be a distinct error code (any integer) for all types of errors that can occur. The error codes does not have to be globally
    // unique. These should be used within either transient or permanent exceptions, see Plugin.cs for examples.
    private const int ERROR_UPSTREAM_UNAVAILBLE = 1001;
    private const int ERROR_INVALID_INPUT = 1002;
    private const int ERROR_NOT_FOUND = 1003;
    private const int ERROR_UNABLE_TO_PARSE_RESPONSE = 1004;
    public const int ERROR_ORGANIZATION_NOT_FOUND = 1;
    public const int ERROR_CCR_UPSTREAM_ERROR = 2;
    public const int ERROR_NO_REPORT_AVAILABLE = 3;
    public const int ERROR_ASYNC_REQUIRED_PARAMS_MISSING = 4;
    public const int ERROR_ASYNC_ALREADY_INITIALIZED = 5;
    public const int ERROR_ASYNC_NOT_INITIALIZED = 6;
    public const int ERROR_AYNC_STATE_STORAGE = 7;
    public const int ERROR_ASYNC_HARVEST_NOT_AVAILABLE = 8;
    public const int ERROR_CERTIFICATE_OF_REGISTRATION_NOT_AVAILABLE = 9;
    public Plugin(IHttpClientFactory httpClientFactory, IOptions<Settings> settings, IEvidenceSourceMetadata evidenceSourceMetadata)

    {
        _client = httpClientFactory.CreateClient("SafeHttpClient");
        _client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "nadobe/data.altinn.no");
        _settings = settings.Value;
        _metadata = evidenceSourceMetadata;
    }

    [Function("Bemanningsforetakregisteret")]
    public async Task<HttpResponseData> Bemanning(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
    {
        _logger = context.GetLogger(context.FunctionDefinition.Name);
        _logger.LogInformation("Running func 'Bemanning'");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesBemanning(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesBemanning(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
        var actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.OrganizationNumber, _client);
        var url = string.Format(_settings.BemanningUrl, actualOrganization.Organisasjonsnummer);
        var content = await MakeRequest<BemanningUnit>(url, actualOrganization.Organisasjonsnummer.ToString());

        var ecb = new EvidenceBuilder(_metadata, "Bemanningsforetakregisteret");
        ecb.AddEvidenceValue($"Organisasjonsnummer", content.Organisasjonsnummer, Metadata.SOURCE);
        ecb.AddEvidenceValue($"Godkjenningsstatus", content.Godkjenningsstatus, Metadata.SOURCE);

        return ecb.GetEvidenceValues();
    }

    [Function("Renholdsregisteret")]
    public async Task<HttpResponseData> Renhold(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
    {
        _logger = context.GetLogger(context.FunctionDefinition.Name);
        _logger.LogInformation("Running func 'Renhold'");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesRenhold(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesRenhold(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
        var actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.OrganizationNumber, _client);
        var url = string.Format(_settings.RenholdUrl, actualOrganization.Organisasjonsnummer);
        var content = await MakeRequest<RenholdUnit>(url, actualOrganization.Organisasjonsnummer.ToString());

        var ecb = new EvidenceBuilder(_metadata, "Renholdsregisteret");
        ecb.AddEvidenceValue($"Organisasjonsnummer", content.Organisasjonsnummer, Metadata.SOURCE);
        ecb.AddEvidenceValue($"Status", content.Status, Metadata.SOURCE);

        if (content.StatusEndret != DateTime.MinValue)
        {
            ecb.AddEvidenceValue($"StatusEndret", content.StatusEndret, Metadata.SOURCE, false);
        }

        return ecb.GetEvidenceValues();
    }
    
    [Function("Bilpleieregisteret")]
    public async Task<HttpResponseData> Bilpleie(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
    {
        _logger = context.GetLogger(context.FunctionDefinition.Name);
        _logger.LogInformation("Running func 'Bilpleie'");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesBilpleie(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesBilpleie(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
        var actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.OrganizationNumber, _client);
        var url = string.Format(_settings.BilpleieUrl, actualOrganization.Organisasjonsnummer);
        var content = await MakeRequest<BilpleieUnit>(url, actualOrganization.Organisasjonsnummer.ToString());

        var ecb = new EvidenceBuilder(_metadata, "Bilpleieregisteret");
        ecb.AddEvidenceValue($"Organisasjonsnummer", content.Data.Organisasjonsnummer, Metadata.SOURCE);
        ecb.AddEvidenceValue($"Registerstatus", content.Data.Registerstatus, Metadata.SOURCE);
        ecb.AddEvidenceValue($"RegisterstatusTekst", content.Data.RegisterstatusTekst, Metadata.SOURCE);
        ecb.AddEvidenceValue($"Godkjenningsstatus", content.Data.Godkjenningsstatus, Metadata.SOURCE);
        ecb.AddEvidenceValue($"Underenheter", JsonConvert.SerializeObject(content.Data.Underenheter), Metadata.SOURCE);
        return ecb.GetEvidenceValues();
    }
    private async Task<T> MakeRequest<T>(string target, string organizationNumber)
    {
        HttpResponseMessage result = null;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, target);
            result = await _client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Target {target} exception: " + ex.Message);
            throw new EvidenceSourcePermanentServerException(ERROR_CCR_UPSTREAM_ERROR, null, ex);
        }

        if (result.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation($"Target {target} not found");
            throw new EvidenceSourcePermanentClientException(ERROR_ORGANIZATION_NOT_FOUND, $"{organizationNumber} could not be found");
        }

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Target {target} failed with status: {result.StatusCode}");
            throw new EvidenceSourceTransientException(ERROR_CCR_UPSTREAM_ERROR, $"Request could not be processed");
        }
        var response = JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
        if (response == null)
        {
            throw new EvidenceSourcePermanentServerException(ERROR_CCR_UPSTREAM_ERROR, "Did not understand the data model returned from upstream source");
        }

        return response;
    }
}

