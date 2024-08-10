using Microsoft.CognitiveServices.Speech;

public class VoiceRecognitionService
{
    private string VOICERECOGAPIKEY = Environment.GetEnvironmentVariable("VOICERECOGAPIKEY");

    public async Task<string> RecognizeText()
    {
        string recognizedText = string.Empty;


        try
        {
            var config = SpeechConfig.FromSubscription(VOICERECOGAPIKEY, "uksouth");
            using var recognizer = new SpeechRecognizer(config);

            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                recognizedText = result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "No speech could be recognized.",
                    "Please try again",
                    "OK");

            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                await Application.Current.MainPage.DisplayAlert(
                $"Speech recognition was canceled. Reason: {cancellation.Reason}, ErrorCode: {cancellation.ErrorCode}, ErrorDetails: {cancellation.ErrorDetails}",
                "Please try again",
                "OK");


            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
               $"An error occurred: {ex.Message}",
               "Please try again",
                "OK");
        }

        return recognizedText;
    }
}
