using System.Text.Json;
using Api.Helpers.cs;
using Fleck;
using lib;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace ws;

public class ClientWantsToDetectImageObjectsDto : BaseDto
{
    public string url { get; set; }
}

public class ServerSendsImageAnalysisToClient : BaseDto
{
    public VisionResponse result { get; set; }
}

public class ClientWantsToDetectImageObjects : BaseEventHandler<ClientWantsToDetectImageObjectsDto>
{
    public override async Task Handle(ClientWantsToDetectImageObjectsDto dto, IWebSocketConnection socket)
    {
        socket.Send(JsonSerializer.Serialize(new ServerSendsImageAnalysisToClient()
        {
            result = await Detect(new VisionRequest() { url = dto.url })
        }));
    }

    private async Task<VisionResponse> Detect(VisionRequest visionRequest)
    {
        using (var httpClient = new HttpClient())
        {
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), 
                       "https://uldahlvision.cognitiveservices.azure.com/vision/v3.2/analyze?language=en&model-version=latest"))
            {
                request.Headers.TryAddWithoutValidation("accept", "application/json");
                request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable(ENV_VAR_KEYS.AZ_VISION.ToString())); 

                request.Content = new StringContent(JsonSerializer.Serialize(visionRequest));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                var response = await httpClient.SendAsync(request);
                if(!response.IsSuccessStatusCode)
                    throw new Exception("Wait a moment - we've used all the free requests for this minute");
                return JsonSerializer.Deserialize<VisionResponse>(await response.Content.ReadAsStringAsync())!;
            }
        }
    }
}

public class VisionRequest
{
    public string url { get; set; }
}

public class Category
{
    public string name { get; set; }
    public double score { get; set; }
}

public class Metadata
{
    public int height { get; set; }
    public int width { get; set; }
    public string format { get; set; }
}

public class VisionResponse
{
    public List<Category> categories { get; set; }
    public string requestId { get; set; }
    public Metadata metadata { get; set; }
    public string modelVersion { get; set; }
}

