using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dan.Plugin.Arbt.Utils;
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

namespace Dan.Plugin.Arbt;
public class Plugin
{
    private ILogger _logger;
    private readonly HttpClient _client;
    private readonly Settings _settings;
    private readonly IEvidenceSourceMetadata _metadata;
    public const int ERROR_ORGANIZATION_NOT_FOUND = 1;
    public const int ERROR_CCR_UPSTREAM_ERROR = 2;
    public Plugin(IHttpClientFactory httpClientFactory, IOptions<Settings> settings, IEvidenceSourceMetadata evidenceSourceMetadata)
    {
        _client = httpClientFactory.CreateClient("SafeHttpClient");
        _settings = settings.Value;
        _metadata = evidenceSourceMetadata;
    }

    [Function("Bemanningsforetakregisteret")]
    public async Task<HttpResponseData> Bemanning(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestData req, FunctionContext context)
    {
        _logger = context.GetLogger(context.FunctionDefinition.Name);
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
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req, () => GetEvidenceValuesBilpleie(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesBilpleie(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        //The registry only has main units, so we have to traverse the enterprise structure in case the subject of the lookup is a subunit
        var actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.OrganizationNumber, _client);
        var url = string.Format(_settings.BilpleieUrl, actualOrganization.Organisasjonsnummer);
        BilpleieUnit content = null;
        var ecb = new EvidenceBuilder(_metadata, "Bilpleieregisteret");
        
        ecb.AddEvidenceValue($"Organisasjonsnummer", actualOrganization.Organisasjonsnummer, Metadata.SOURCE);

        try
        {
            content = await MakeRequest<BilpleieUnit>(url, actualOrganization.Organisasjonsnummer.ToString());
        }
        catch (EvidenceSourcePermanentClientException) { }

        ecb.AddEvidenceValue($"Registerstatus", content != null ? content.Data.Registerstatus : -1, Metadata.SOURCE);
        ecb.AddEvidenceValue($"RegisterstatusTekst", content != null ? content.Data.RegisterstatusTekst : "Ikke funnet", Metadata.SOURCE);
        ecb.AddEvidenceValue($"Godkjenningsstatus", content != null ? content.Data.Godkjenningsstatus : "Ikke registrert", Metadata.SOURCE);

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

