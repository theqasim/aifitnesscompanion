using aifitnesscompanion.ViewModels;

namespace aifitnesscompanion.Views;

public partial class CalorieCalculator : ContentPage
{
    public CalorieCalculator()
    {
        InitializeComponent();
        BindingContext = new CalorieCalculatorViewModel();

    }

}