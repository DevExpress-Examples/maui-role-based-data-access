using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MAUI.Models;
using ON = DevExpress.Maui.Core.On;
namespace MAUI.Services;

public class WebAPIService : IDataStore<Post> {
    private readonly HttpClient HttpClient = new() { Timeout = new TimeSpan(0, 0, 10) };
#if DEBUG
    public static readonly string ApiUrl = ON.Platform("http://10.0.2.2:5000/api/", "http://localhost:5000/api/");
#else
    public static readonly string ApiUrl = ON.Platform("https://10.0.2.2:5001/api/", "https://localhost:5001/api/");
#endif
    private readonly string _postEndPointUrl;
    private const string ApplicationJson = "application/json";

    public WebAPIService() {
        _postEndPointUrl = ApiUrl + "odata/" + nameof(Post);
    }

    public async Task<bool> UserCanDeletePostAsync() {
        var jsonString = await HttpClient.GetStringAsync($"{ApiUrl}CustomEndpoint/CanDeletePost?typeName={typeof(Post).Name}");
        return (bool)JsonNode.Parse(jsonString);
    }
    public async Task<bool> DeletePostAsync(int postId) {
        var response = await HttpClient.DeleteAsync($"{_postEndPointUrl}({postId})");
        return response.IsSuccessStatusCode;
    }

    public async Task<Post> GetItemAsync(string id)
        => (await RequestItemsAsync($"?$filter={nameof(Post.PostId)} eq {id}")).FirstOrDefault();

    public async Task<IEnumerable<Post>> GetItemsAsync(bool forceRefresh = false)
        => await RequestItemsAsync($"?$expand=Author($expand=Photo)");


    private async Task<IEnumerable<Post>> RequestItemsAsync(string query = null) {
        return JsonNode.Parse(await HttpClient.GetStringAsync($"{_postEndPointUrl}{query}"))!["value"].Deserialize<IEnumerable<Post>>();
    }

    public async Task<string> Authenticate(string userName, string password) {
        HttpResponseMessage tokenResponse;
        try {
            tokenResponse = await RequestTokenAsync(userName, password);
        }
        catch (Exception) {
#if DEBUG
            await Application.Current.MainPage.DisplayAlert("Couldn't reach the WebAPI service", "Potential reasons: \n\n- The WebAPI project is not started. Please right-click the WebAPI project and choose Debug -> Start New Instance \n\n- You debug the project using a physical device. Please try using an emulator \n\n- IIS Express on Windows blocks the access. Please follow the recommendations in the example description", "OK");
#endif
            return "An error occurred when processing the request";
        }
        if (tokenResponse.IsSuccessStatusCode) {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await tokenResponse.Content.ReadAsStringAsync());
            return null;
        }
        else {
            return await tokenResponse.Content.ReadAsStringAsync();
        }
    }

    private async Task<HttpResponseMessage> RequestTokenAsync(string userName, string password) {
        return await HttpClient.PostAsync($"{ApiUrl}Authentication/Authenticate",
                                            new StringContent(JsonSerializer.Serialize(new { userName, password = $"{password}" }), Encoding.UTF8,
                                            ApplicationJson));
    }

    public async Task<ApplicationUser> CurrentUser() {
        ApplicationUser user;
        try {
            string  stringResponse = await HttpClient.GetStringAsync($"{ApiUrl}CustomEndpoint/CurrentUser");
            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            };
            user = JsonSerializer.Deserialize<ApplicationUser>(stringResponse, options);
        }
        catch (Exception) {
            return null;
        }
        return user;
    }
}

