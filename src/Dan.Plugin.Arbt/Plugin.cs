using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Common;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Arbt.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dan.Plugin.Arbt;

public class Plugin
{
    private readonly IEvidenceSourceMetadata _evidenceSourceMetadata;
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly Settings _settings;

    // The datasets must supply a human-readable source description from which they originate. Individual fields might come from different sources, and this string should reflect that (ie. name all possible sources).
    public const string SourceName = "Digitaliseringsdirektoratet";

    // The function names (ie. HTTP endpoint names) and the dataset names must match. Using constants to avoid errors.
    public const string SimpleDatasetBemanning = "Bemanningsforetakregisteret";
    public const string SimpleDatasetRenhold = "Renholdsregisteret";

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

    public Plugin(
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        IOptions<Settings> settings,
        IEvidenceSourceMetadata evidenceSourceMetadata)
    {
        _client = httpClientFactory.CreateClient(Constants.SafeHttpClient);
        _logger = loggerFactory.CreateLogger<Plugin>();
        _settings = settings.Value;
        _evidenceSourceMetadata = evidenceSourceMetadata;
    }

    [Function(SimpleDatasetBemanning)]
    public async Task<HttpResponseData> GetSimpleDatasetAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext context)
    {
        _logger.LogInformation("Running func 'Bemanning'");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req,
            () => GetEvidenceValuesSimpledataset(evidenceHarvesterRequest));
    }

    [Function(SimpleDatasetRenhold)]
    public async Task<HttpResponseData> GetRichDatasetAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext context)
    {
        _logger.LogInformation("Running func 'Renhold'");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var evidenceHarvesterRequest = JsonConvert.DeserializeObject<EvidenceHarvesterRequest>(requestBody);

        return await EvidenceSourceResponse.CreateResponse(req,
            () => GetEvidenceValuesRichDataset(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesSimpledataset(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        var actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.OrganizationNumber, _client);
        dynamic content = await MakeRequest<String>(string.Format(_settings.BemanningUrl, actualOrganization.Organisasjonsnummer));

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, SimpleDatasetBemanning);
        ecb.AddEvidenceValue($"Organisasjonsnummer", content.Organisasjonsnummer, SourceName);
        ecb.AddEvidenceValue($"Godkjenningsstatus", content.Godkjenningsstatus, SourceName);

        return ecb.GetEvidenceValues();
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesRichDataset(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {

        var actualOrganization = await BRUtils.GetMainUnit(evidenceHarvesterRequest.OrganizationNumber, _client);
        dynamic content = await MakeRequest<String>(string.Format(_settings.RenholdUrl, actualOrganization.Organisasjonsnummer));

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, SimpleDatasetRenhold);
        ecb.AddEvidenceValue($"Organisasjonsnummer", content.Organisasjonsnummer, SourceName);
        ecb.AddEvidenceValue($"Status", content.Status, SourceName);

        var statusChanged = Convert.ToDateTime(content.StatusEndret);

        if (statusChanged != DateTime.MinValue)
        {
            ecb.AddEvidenceValue($"StatusEndret", statusChanged, SourceName, false);

        }

        return ecb.GetEvidenceValues();
    }

    private async Task<T> MakeRequest<T>(string target)
    {
        HttpResponseMessage result;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, target);
            result = await _client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            throw new EvidenceSourceTransientException(ERROR_UPSTREAM_UNAVAILBLE, "Error communicating with upstream source", ex);
        }

        if (!result.IsSuccessStatusCode)
        {
            throw result.StatusCode switch
            {
                HttpStatusCode.NotFound => new EvidenceSourcePermanentClientException(ERROR_NOT_FOUND, "Upstream source could not find the requested entity (404)"),
                HttpStatusCode.BadRequest => new EvidenceSourcePermanentClientException(ERROR_INVALID_INPUT, "Upstream source indicated an invalid request (400)"),
                _ => new EvidenceSourceTransientException(ERROR_UPSTREAM_UNAVAILBLE, $"Upstream source retuned an HTTP error code ({(int)result.StatusCode})")
            };
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError("Unable to parse data returned from upstream source: {exceptionType}: {exceptionMessage}", ex.GetType().Name, ex.Message);
            throw new EvidenceSourcePermanentServerException(ERROR_UNABLE_TO_PARSE_RESPONSE, "Could not parse the data model returned from upstream source", ex);
        }
    }
}
