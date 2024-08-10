using aifitnesscompanion.Helpers;
using aifitnesscompanion.Services;
using aifitnesscompanion.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;


namespace aifitnesscompanion.Views;


public partial class GymLocator : ContentPage
{
    [Obsolete]
    public GymLocator()
    {
        InitializeComponent();
        BindingContext = new GymLocatorViewModel();

        MessagingCenter.Subscribe<GymLocatorViewModel, Location>(this, "UpdateMapRegion", (sender, location) =>
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                
                map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(200)));
                VibrationHelper.Vibrate(TimeSpan.FromSeconds(0.1));
            });
        });


        MessagingCenter.Subscribe<GymLocatorViewModel, (Location Location, Distance Radius, string Label, PinType Type)>(this, MapMessages.UpdateMapRegionAndAddPin, (sender, args) =>
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                map.MoveToRegion(MapSpan.FromCenterAndRadius(args.Location, args.Radius));
                var pin = new Pin
                {
                    Label = args.Label,
                    Location = args.Location,
                    Type = args.Type,
                   
                    
                };
                map.Pins.Add(pin);
            });
        });
    }

    private void OnGymsSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;

        var selectedGym = e.SelectedItem as GymLocatorViewModel.GymDistanceInfo;

        if (BindingContext is GymLocatorViewModel viewModel)
        {
            viewModel.MoveToSelectedGym(selectedGym);
        }

        gymsListView.SelectedItem = null;
    }

    [Obsolete]
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<GymLocatorViewModel, (Location, Distance, string, PinType)>(this, "UpdateMapRegionAndAddPin");
        MessagingCenter.Unsubscribe<GymLocatorViewModel, Location>(this, "UpdateMapRegion");
    }
}
