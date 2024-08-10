using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

public class AIModelService
{
    public class ConversationItem
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    private readonly HttpClient _httpClient;
    private string ApiUrl = Environment.GetEnvironmentVariable("AIAPIURL");
    private string ApiKey = Environment.GetEnvironmentVariable("AIAPIKEY");

    public AIModelService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> GetAIResponseFromOCR(string ocrText)
    {
        try
        {
            var convoDataStructure = new List<ConversationItem>
        {
            new ConversationItem
            {
                Role = "system",
                Content = "You are to act as an expert dietitian & nutritionist give the average calories amount for the food item provided, the food is being extracted from users taking photos of of menu items. You will receive the extracted text, if any. In the event of a the user not providing a food menu item, ask them to retake a photo, strictly only provide average calories for meals or food. Your only function is to await the user taking a photo of a menu item. Do not answer anything unrelated to this purpose. Keep your answers concise and to the point."
            },
            new ConversationItem
            {
                Role = "user",
                Content = ocrText
            }
        };

            var requestData = new
            {
                model = "gpt-3.5-turbo-16k-0613",
                messages = convoDataStructure.Select(item => new { role = item.Role, content = item.Content }).ToList(),
                temperature = 0.7,
                max_tokens = 150,
                top_p = 1.0,
                frequency_penalty = 0.0,
                presence_penalty = 0.0
            };

            var jsonContent = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var aiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                var choiceContent = aiResponse.choices[0].message.content.ToString();
                return aiResponse.choices[0].message.content;
            }
            else
            {
                return $"Error: {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            return $"Exception occurred: {ex.Message}";
        }
    }

}
