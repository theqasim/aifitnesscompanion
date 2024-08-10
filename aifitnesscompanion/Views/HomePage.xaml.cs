using aifitnesscompanion.ViewModels;

namespace aifitnesscompanion.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new HomePageViewModel();
    }

    

}
