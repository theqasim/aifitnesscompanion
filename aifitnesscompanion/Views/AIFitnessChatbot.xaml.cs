using aifitnesscompanion.ViewModels;

namespace aifitnesscompanion.Views;

public partial class AIFitnessChatbot : ContentPage
{
	public AIFitnessChatbot()
	{
        AIFitnessChatbotViewModel viewModel = new AIFitnessChatbotViewModel(new VoiceRecognitionService());
        BindingContext = viewModel;
        InitializeComponent();
        
    }


}
