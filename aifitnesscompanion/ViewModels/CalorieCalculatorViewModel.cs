using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using aifitnesscompanion.Helpers;

namespace aifitnesscompanion.ViewModels;

public partial class CalorieCalculatorViewModel : ObservableObject
{

    [ObservableProperty]
    private bool _isOverlayVisible;

    [ObservableProperty]
    private bool _captureImageStatus = true;

    [ObservableProperty]
    private string _captureImageButtonText = "Capture Image";


    private string apiKey = Environment.GetEnvironmentVariable("AZUREAPIKEY");
    private string azureEndpoint = Environment.GetEnvironmentVariable("AZUREENDPOINT");


    private double _layoutOpacity = 1.0;
    public double LayoutOpacity
    {
        get => _layoutOpacity;
        set
        {
            _layoutOpacity = value;
            OnPropertyChanged();
        }
    }

    public ICommand SwipeCommand { get; private set; }


    [ObservableProperty]
    private bool _isCalculatingVisible;

    [ObservableProperty]
    private string _aiResponseText;

    public event PropertyChangedEventHandler PropertyChanged;

    public ICommand CaptureImageCommand { get; private set; }

    public CalorieCalculatorViewModel()
    {
        CaptureImageCommand = new Command(async () => await OnCapturePhotoClicked());
        SwipeCommand = new Command<string>(async (direction) =>
        {
            if (direction == "Right")
            {
                LayoutOpacity = 1.0;
                for (double i = 1.0; i >= 0; i -= 0.10)
                {
                    LayoutOpacity = i;
                    await Task.Delay(5);
                }
                await NavigationHelper.HandleSingleSwipe("///GymLocator");
                LayoutOpacity = 1.0;
            }
        });
    }
    

    private async Task OnCapturePhotoClicked()
    {
        var permissionChecks = new List<Func<Task<bool>>>
        {
            PermissionsAndInternetHandlers.CheckInternetStatus,
            PermissionsAndInternetHandlers.CheckAndRequestCameraPermission,
            PermissionsAndInternetHandlers.CheckAndRequestStoragePermission
        };

        foreach (var check in permissionChecks)
        {
            var hasPermission = await check();
            if (!hasPermission)
            {
                return;
            }
        }

        try
        {
            AiResponseText = "";
            var photo = await CapturePhoto();
            if (photo != null)
            {
                CaptureImageButtonText = "Processing Image";
                CaptureImageStatus = false;
                IsOverlayVisible = true;
                IsCalculatingVisible = true;

                using (var stream = await photo.OpenReadAsync())
                {
                    stream.Position = 0;
                    var text = await ExtractTextFromImageAsync(stream);

                    var aiModelService = new AIModelService();
                    var aiResponse = await aiModelService.GetAIResponseFromOCR(text);

                    AiResponseText = aiResponse;

                }

                IsOverlayVisible = false;
                IsCalculatingVisible = false;
                CaptureImageButtonText = "Capture Image";
                CaptureImageStatus = true;

            }
            else
            {
                AiResponseText = "Failed to capture photo, please try again.";
            }
        }
        catch (Exception ex)
        {
            IsOverlayVisible = false;
            IsCalculatingVisible = false;
            await Application.Current.MainPage.DisplayAlert("Error", "Unable to take the photo using your device " + ex.Message, "Cancel");
        }
    }

    private async Task<FileResult> CapturePhoto()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            return photo;
        }
        return null;
    }

    private async Task<string> ExtractTextFromImageAsync(Stream imageStream)
    {
        imageStream.Position = 0;

        try
        {
            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = azureEndpoint
            };

            var textHeaders = await client.ReadInStreamAsync(imageStream);
            string operationLocation = textHeaders.OperationLocation;

            string operationId = operationLocation.Split('/').Last();

            ReadOperationResult results;
            do
            {
                await Task.Delay(1000); 
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted);

            var textResult = new StringBuilder();
            if (results.Status == OperationStatusCodes.Succeeded)
            {
                foreach (var page in results.AnalyzeResult.ReadResults)
                {
                    foreach (var line in page.Lines)
                    {
                        textResult.AppendLine(line.Text);
                    }
                }
            }
            return textResult.ToString();
        }
        catch (Exception ex)
        {
            return $"OCR processing failed: {ex.Message}";
        }
    }
}