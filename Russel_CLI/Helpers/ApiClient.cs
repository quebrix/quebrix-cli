﻿using Newtonsoft.Json;
using RestSharp;
using Russel_CLI.Extensions;
using System.Diagnostics.Metrics;
using System.Text;

namespace Russel_CLI.Helpers;

public class ApiClient
{
    private readonly string _baseUrl;
    private readonly RestClient _client;

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _client = new RestClient(baseUrl);
    }

    public async Task CheckConnection()
    {
        var url = $"/api/check";
        var request = new RestRequest(url, Method.Get);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            "tock".WriteResponse();
        }
        else
        {
            response.ErrorMessage.WriteError();
        }
    }

    public async Task Set(string cluster, string key, string value)
    {
        var url = "/api/set";
        var request = new RestRequest(url, Method.Post);

        var setRequest = new SetRequest
        {
            cluster = cluster,
            key = key,
            value = Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            $"Value [{value}] set on cluster [{cluster}]".WriteResponse();
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }
    }

    public async Task<string> Get(string cluster, string key)
    {
        try
        {
            var url = $"/api/get/{cluster}/{key}";
            var request = new RestRequest(url, Method.Get);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<byte[]>>(response.Content);
                if (apiResponse.IsSuccess)
                {
                    return Encoding.UTF8.GetString(apiResponse.Data).DecodeBase64ToString();
                }
                else
                {
                    "Failed to get value".WriteError();
                }
            }
            return response.ErrorMessage;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting value: {ex.Message}");
        }
    }

    public async Task Delete(string cluster, string key)
    {
        var url = $"/api/delete/{cluster}/{key}";
        var request = new RestRequest(url, Method.Delete);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            $"Deleted {key} from cluster [{cluster}]".WriteResponse();
        }
        else
        {
            $"Error deleting value: {response.ErrorMessage}".WriteError();
        }
    }

    public async Task<List<string>> GetKeysOfCluster(string clusterName)
    {
        var url = $"/api/get_keys/{clusterName}";
        var request = new RestRequest(url, Method.Get);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<string>>>(response.Content);
            return apiResponse.Data;
        }
        else
        {
            throw new Exception($"Error getting keys of  cluster: {response.ErrorMessage}");
        }
    }

    public async Task SetCluster(string cluster)
    {
        var url = $"/api/set_cluster/{cluster}";
        var request = new RestRequest(url, Method.Post);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            $"cluster [{cluster}] set".WriteResponse();
        }
        else
        {
            $"Error set cluster: {response.ErrorMessage}".WriteError();
        }
    }
    public async Task ClearCluster(string cluster)
    {
        var url = $"/api/clear_cluster/{cluster}";
        var request = new RestRequest(url, Method.Delete);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            $"Cleared cluster [{cluster}]".WriteResponse();
        }
        else
        {
            $"Error clearing cluster: {response.ErrorMessage}".WriteError();
        }
    }

    public async Task<List<string>> GetAllClusters()
    {
        var url = $"/api/get_clusters";
        var request = new RestRequest(url, Method.Get);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            try
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<string>>>(response.Content);
                return apiResponse.Data;
            }
            catch
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
                if (!string.IsNullOrEmpty(apiResponse.Data))
                {
                    return new List<string>() { apiResponse.Data };
                }
                else
                {
                    return new List<string>() { "no cluster found" };
                }
            }
        }
        else
        {
            throw new Exception($"Error getting clusters: {response.ErrorMessage}");
        }
    }

}
//requests
public class ApiResponse<T>
{
    [JsonProperty("is_success")]
    public bool IsSuccess { get; set; }

    [JsonProperty("data")]
    public T Data { get; set; }
}

public class SetRequest
{
    [JsonProperty("cluster")]
    public string cluster { get; set; }

    [JsonProperty("key")]
    public string key { get; set; }

    [JsonProperty("value")]
    public string value { get; set; }
}

