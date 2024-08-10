using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using aifitnesscompanion.Helpers;

namespace aifitnesscompanion.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand OpenYouTubeCommand { get; }
        public ICommand SwipeCommand { get; private set; }

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

        public HomePageViewModel()
        {
            SwipeCommand = new Command<string>(async (direction) => {
                if (direction == "Left")
                {
                    LayoutOpacity = 1.0;
                    for (double i = 1.0; i >= 0; i -= 0.10)
                    {
                        LayoutOpacity = i;
                        await Task.Delay(5);
                    }
                    await NavigationHelper.HandleSingleSwipe("///AIFitnessCoach");
                    LayoutOpacity = 1.0;
                }
            });



            OpenYouTubeCommand = new Command(async () =>
            {
                var youtubeUrl = "https://youtu.be/5QUuMx8gYZA";
                try
                {

                    await Browser.Default.OpenAsync(new Uri(youtubeUrl), BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Unable to open the YouTube video. " + ex.Message, "Cancel");
                }
            });
        }
    }
}
