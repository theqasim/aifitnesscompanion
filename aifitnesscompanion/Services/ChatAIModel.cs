using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using static aifitnesscompanion.ViewModels.AIFitnessChatbotViewModel;

namespace aifitnesscompanion.Services
{
    public class ChatAIModel
    {
        private string apiUrl = Environment.GetEnvironmentVariable("AIAPIURL");
        private string apiKey = Environment.GetEnvironmentVariable("AIAPIKEY");
        private HttpClient httpClient = new HttpClient();

        public async Task<string> GetAIResponse(ObservableCollection<MessageViewModel> messages)
        {
            try
            {
                var requestData = new
                {
                    model = "gpt-3.5-turbo-16k-0613",
                    messages = messages.Select(item => new { role = item.Role.ToLower(), content = item.Content }).ToList(),
                    temperature = 0.7,
                    max_tokens = 150,
                    top_p = 1.0,
                    frequency_penalty = 0.0,
                    presence_penalty = 0.0
                };

                var requestBody = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    var chatbotResponseContent = (string)apiResponse.choices[0].message.content;
                    return chatbotResponseContent; 
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return "Error: Unable to get a response from the AI."; 
                }
            }
            catch (Exception ex)
            {
                return "Error: An exception occurred while trying to get a response from the AI." + ex; 
            }
        }
    }
}
