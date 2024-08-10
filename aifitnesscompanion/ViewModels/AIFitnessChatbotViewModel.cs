using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using aifitnesscompanion.Helpers;
using aifitnesscompanion.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace aifitnesscompanion.ViewModels;

public partial class AIFitnessChatbotViewModel : ObservableObject
{
    private readonly VoiceRecognitionService _voiceRecognitionService;
    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<MessageViewModel> Messages { get; } = new ObservableCollection<MessageViewModel>();

    public bool ShouldAnimate { get; set; } = true; 

    public ICommand SwipeCommand { get; private set; }

    [ObservableProperty]
    private string _recognizedText;

    [ObservableProperty]
    private bool _isListening;

    [ObservableProperty]
    private bool _isAICoachTyping = false;

    [ObservableProperty]
    private double _layoutOpacity = 1.0;

    [ObservableProperty]
    private bool _sendButtonStatus;

    [ObservableProperty]
    private bool _chatEntryStatus = true;

    private ObservableCollection<MessageViewModel> _uiMessages;
    public ObservableCollection<MessageViewModel> UIMessages
    {
        get => _uiMessages;
        private set => SetProperty(ref _uiMessages, value);
    }

    private readonly ChatAIModel _chatAIModel = new ChatAIModel();

    [ObservableProperty]
    private bool _microphoneButtonStatus = true;

    [ObservableProperty]
    private string _messageEntryText;

    [ObservableProperty]
    private CollectionView _messagesList;

    public ICommand AppearingCommand { get; private set; }

    partial void OnMessageEntryTextChanged(string value)
    {
        SendButtonStatus = !string.IsNullOrWhiteSpace(value);
    }


    public AIFitnessChatbotViewModel(VoiceRecognitionService voiceRecognitionService)
    {
        
        SwipeCommand = new Command<string>(async (direction) =>
        {

            if (direction == "Left")
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
            else if (direction == "Right")
            {
                LayoutOpacity = 1.0;
                for (double i = 1.0; i >= 0; i -= 0.10)
                {
                    LayoutOpacity = i;
                    await Task.Delay(5);
                }
                await NavigationHelper.HandleSingleSwipe("///MainPage");
                LayoutOpacity = 1.0;

            }

        });
        _voiceRecognitionService = voiceRecognitionService;
        Messages.CollectionChanged += Messages_CollectionChanged;
        _uiMessages = new ObservableCollection<MessageViewModel>(Messages.Where(m => m.Role != "system"));
        SetupInitialConversation();
    }



    public class MessageViewModel : ObservableObject
    {
        public string Role { get; set; } 
        public string Content { get; set; }
    }

    private void SetupInitialConversation()
    {
        Messages.Add(new MessageViewModel
        {
            Role = "system",
            Content = "You are an expert in fitness. Based on user inputs, please provide insightful information guiding the user on their fitness journey. Do not answer anything unrelated to fitness. Keep your answers concise and to the point."
        });
    }

    private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UIMessages.Clear();
        foreach (var message in Messages.Where(m => m.Role != "system"))
        {
            UIMessages.Add(message);
        }
    }

    public ICommand SendVoiceMessageCommand => new Command(async () =>
    {
        bool internetStatus = await PermissionsAndInternetHandlers.CheckInternetStatus();
        if (!internetStatus)
        {
            return; 
        }

        bool isMicrophoneGranted = await PermissionsAndInternetHandlers.CheckAndRequestMicrophonePermission();
        if (isMicrophoneGranted)
        {
            ChatEntryStatus = false;
            MicrophoneButtonStatus = false;
            IsListening = true;

            MessageEntryText = await _voiceRecognitionService.RecognizeText();
            IsListening = false;
            MicrophoneButtonStatus = true;
            ChatEntryStatus = true;
        }
    });


    public ICommand SendMessageCommand => new Command<string>(async (message) =>
    {
        bool internetStatus = await PermissionsAndInternetHandlers.CheckInternetStatus();
        if (!internetStatus)
        {
            return; 
        }

        Messages.Add(new MessageViewModel { Role = "User", Content = MessageEntryText });
        MessageEntryText = string.Empty;
        ChatEntryStatus = false;
        IsAICoachTyping = true;
        SendButtonStatus = false;

        var aiResponse = await _chatAIModel.GetAIResponse(Messages);

        if (aiResponse != null)
        {
            Messages.Add(new MessageViewModel { Role = "assistant", Content = aiResponse });
        }
        else
        {
            Messages.Add(new MessageViewModel { Role = "assistant", Content = "Error" });
        }
        IsAICoachTyping = false;
        ChatEntryStatus = true;
        VibrationHelper.Vibrate();
    });



}

