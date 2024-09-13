using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using quebrix.Helpers;
using RestSharp;
using Russel_CLI.Extensions;
using System.Diagnostics.Metrics;
using System.Text;
using System.Web;

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
        var url = $"/api/ping";
        var request = new RestRequest(url, Method.Get);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            "pong".WriteResponse();
        }
        else
        {
            "russel is not run check russel first.".WriteError();
        }
    }


    private string MakeAuth(string username, string password) => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));


    public async Task Set(string cluster, string key, string value, string userName, string password, long? expireTime = null)
    {
        var url = "/api/set";
        var request = new RestRequest(url, Method.Post);
        var setRequest = new SetRequest
        {
            cluster = cluster,
            key = key,
            value = value,
            ttl = expireTime
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                $"Value set on cluster [{cluster}]".WriteResponse();
            }
            else
            {
                "access denied login first".WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }
    }

    public async Task CopyCluster(string srcCluster, string destCluster,string userName, string password)
    {
        var url = "/api/copy_cluster";
        var request = new RestRequest(url, Method.Post);
        var setRequest = new MoveClusterRequest
        {
            DestCluster = destCluster,
            SrcCluster = srcCluster
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                result.Data.WriteResponse();
            }
            else
            {
                "src cluster not found".WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }
    }

    public async Task TypeOfKey(string cluster, string key, string userName, string password)
    {
        var url = "/api/typeof";
        var request = new RestRequest(url, Method.Post);
        var setRequest = new TypeOfKeyRequest
        {
            Cluster = cluster,
            key = key
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                result.Data.WriteResponse();
            }
            else
            {
                result.Data.WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }
    }

    public async Task Exists(string cluster, string key, string userName, string password)
    {
        var url = "/api/exists";
        var request = new RestRequest(url, Method.Post);
        var setRequest = new TypeOfKeyRequest
        {
            Cluster = cluster,
            key = key
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                result.Data.WriteResponse();
            }
            else
            {
                result.Data.WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }
    }

    public async Task Expire(string cluster, string key,long ttl, string userName, string password)
    {
        var url = "/api/expire";
        var request = new RestRequest(url, Method.Post);
        var setRequest = new ExpireKeyRequest
        {
            Cluster = cluster,
            key = key,
            ttl = ttl
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                result.Data.ToString().WriteResponse();
            }
            else
            {
                result.Data.ToString().WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }
    }


    public async Task MoveCluster(string srcCluster, string destCluster, string userName, string password)
    {
        var url = "/api/move_cluster";
        var request = new RestRequest(url, Method.Post);
        var setRequest = new MoveClusterRequest
        {
            DestCluster = destCluster,
            SrcCluster = srcCluster
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                result.Data.WriteResponse();
            }
            else
            {
                "src cluster not found".WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }
    }

    public async Task INCR(string cluster, string key, int? value, string userName, string password)
    {
        var url = "/api/incr";
        var request = new RestRequest(url, Method.Post);
        var setRequest = new SetINCR_DECRRequest
        {
            cluster = cluster,
            key = key,
            value = value,
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                $"INCR set on cluster [{cluster}]".WriteResponse();
            }
            else
            {
                "access denied login first".WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }
    }

    public async Task DECR(string cluster, string key, int? value, string userName, string password)
    {
        var url = "/api/decr";
        var request = new RestRequest(url, Method.Post);
        var setRequest = new SetINCR_DECRRequest
        {
            cluster = cluster,
            key = key,
            value = value,
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                $"DECR set on cluster [{cluster}]".WriteResponse();
            }
            else
            {
                "key not found".WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }
    }


    public async Task LoadUserFromFile(string userName, string password)
    {
        var url = "/api/load_users_from_file";
        var request = new RestRequest(url, Method.Post);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                result.Data.WriteResponse();
            }
            else
            {
                "access denied login first".WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }

    }

    public async Task ListOfUsers(string userName, string password)
    {
        var url = "/api/load_users";
        var request = new RestRequest(url, Method.Get);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<List<UserResponse>>>(response.Content);
            if (result.IsSuccess)
            {
                foreach (var item in result.Data)
                {
                    $"{item.UserName}:{item.Role}".WriteResponse();
                }

            }
            else
            {
                "access denied login first".WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }

    }


    public async Task WhoAmI(string userName, string password)
    {
        var url = "/api/who_am_i";
        var request = new RestRequest(url, Method.Get);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<UserResponse>>(response.Content);
            if (result.IsSuccess)
            {
                $"userName:{result.Data.UserName} \nrole:{result.Data.Role}".WriteResponse();

            }
            else
            {
                "access denied login first".WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }

    }

    public async Task DeleteUser(string deleteUserName, string userName, string password)
    {
        var url = $"/api/delete_user/{deleteUserName.EncodeUrl()}";
        var request = new RestRequest(url, Method.Delete);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");

        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                $"{result.Data}".WriteResponse();
            }
            else
            {
                "access denied login first".WriteError();
            }
        }
        else
        {
            $"Error setting value: {response.ErrorMessage}".WriteError();
        }

    }

    public async Task AddUser(string userName, string password, string role, string reqUser, string reqPassword)
    {
        var url = "/api/add_user";
        if (role != "Admin" && role != "Developer")
            "wrong role set Admin or Developer".WriteError();
        var request = new RestRequest(url, Method.Post);
        var setRequest = new AuthenticateRequest
        {
            password = password,
            UserName = userName,
            Role = role
        };
        request.AddBody(JsonConvert.SerializeObject(setRequest));
        var credentials = MakeAuth(reqUser, reqPassword);
        request.AddHeader("Authorization", $"{credentials}");
        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
                $"{result.Data}".WriteGreen();
            else
                $"{result.Data}".WriteError();

        }
        else
            "error to set profile".WriteError();
    }

    public async Task<bool> Authenticate(string userName, string password)
    {
        var url = "/api/login";
        var request = new RestRequest(url, Method.Post);
        var setRequest = new AuthenticateRequest
        {
            password = password,
            UserName = userName,
            Role = string.Empty
        };
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(JsonConvert.SerializeObject(setRequest));
        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful)
        {
            var result = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
            if (result.IsSuccess)
            {
                return true;
            }

            else
            {
                return false;
            }

        }
        else
        {
            " ERROR => check server connection".WriteError();
            return false;
        }

    }

    public async Task<string> Get(string cluster, string key, string userName, string password)
    {
        try
        {

            var url = $"/api/get/{cluster.EncodeUrl()}/{key.EncodeUrl()}";
            var request = new RestRequest(url, Method.Get);
            var credentials = MakeAuth(userName, password);
            request.AddHeader("Authorization", $"{credentials}");
            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ValueResult>>(response.Content);
                if (apiResponse.IsSuccess)
                {
                    var result = string.Empty;
                    if (apiResponse.Data.ValueType == "Int")
                        result = BitConverter.ToInt32(apiResponse.Data.Value).ToString();
                    else
                    {
                        result = Encoding.UTF8.GetString(apiResponse.Data.Value);
                    }
                    return result;
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

    public async Task Delete(string cluster, string key, string userName, string password)
    {
        var url = $"/api/delete/{cluster.EncodeUrl()}/{key.EncodeUrl()}";
        var request = new RestRequest(url, Method.Delete);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");
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

    public async Task<List<string>> GetKeysOfCluster(string clusterName, string userName, string password)
    {
        var url = $"/api/get_keys/{clusterName.EncodeUrl()}";
        var request = new RestRequest(url, Method.Get);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");
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

    public async Task SetCluster(string cluster, string userName, string password)
    {
        var url = $"/api/set_cluster/{cluster.EncodeUrl()}";
        var request = new RestRequest(url, Method.Post);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");
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
    public async Task ClearCluster(string cluster, string userName, string password)
    {
        var url = $"/api/clear_cluster/{cluster.EncodeUrl()}";
        var request = new RestRequest(url, Method.Delete);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");
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

    public async Task<List<string>> GetAllClusters(string userName, string password)
    {
        var url = $"/api/get_clusters";
        var request = new RestRequest(url, Method.Get);
        var credentials = MakeAuth(userName, password);
        request.AddHeader("Authorization", $"{credentials}");
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

public class UserResponse
{
    [JsonProperty("password")]
    public object Password { get; set; }

    [JsonProperty("username")]
    public string UserName { get; set; }

    [JsonProperty("role")]
    public string Role { get; set; }
}
public class ApiResponse<T>
{
    [JsonProperty("is_success")]
    public bool IsSuccess { get; set; }

    [JsonProperty("data")]
    public T Data { get; set; }
}

public class SetINCR_DECRRequest
{
    [JsonProperty("cluster")]
    public string cluster { get; set; }

    [JsonProperty("key")]
    public string key { get; set; }

    [JsonProperty("value")]
    public int? value { get; set; }

}
public class MoveClusterRequest
{
    [JsonProperty("src_cluster")]
    public string SrcCluster { get; set; }

    [JsonProperty("desc_cluster")]
    public string DestCluster { get; set; }

}


public class TypeOfKeyRequest
{
    [JsonProperty("cluster")]
    public string Cluster { get; set; }

    [JsonProperty("key")]
    public string key { get; set; }

}

public class ExpireKeyRequest
{
    [JsonProperty("cluster")]
    public string Cluster { get; set; }

    [JsonProperty("key")]
    public string key { get; set; }
    [JsonProperty("ttl")]
    public long ttl { get; set; }

}

public class SetRequest
{
    [JsonProperty("cluster")]
    public string cluster { get; set; }

    [JsonProperty("key")]
    public string key { get; set; }

    [JsonProperty("value")]
    public string value { get; set; }

    [JsonProperty("ttl")]
    public long? ttl { get; set; }
}

public class ValueResult
{
    [JsonProperty("value")]
    public byte[] Value { get; set; }
    [JsonProperty("value_type")]
    public string ValueType { get; set; }
}

public class AuthenticateRequest
{
    [JsonProperty("username")]
    public string UserName { get; set; }

    [JsonProperty("password")]
    public string password { get; set; }
    [JsonProperty("role")]
    public string Role { get; set; }
}

