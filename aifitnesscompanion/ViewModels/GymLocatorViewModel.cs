using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using aifitnesscompanion.Helpers;
using System.Windows.Input;
using aifitnesscompanion.Services;

namespace aifitnesscompanion.ViewModels;

public partial class GymLocatorViewModel : ObservableObject
{
    public Command LocateGymsCommand { get; }

    [ObservableProperty]
    private Color _buttonTextColor = Color.FromRgb(255, 255, 255);

    [ObservableProperty]
    private bool _isUserLocationVisible = false;

    [ObservableProperty]
    private string _buttonText = "Locate Gyms Nearby!";

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



    [ObservableProperty]
    private bool _isMapVisible = false;

    [ObservableProperty]
    private Color _buttonColor = Color.FromRgb(25, 25, 112);

    private HttpClient _httpClient = new HttpClient();

    public ICommand SwipeCommand { get; private set; }


    public class GymDistanceInfo
    {
        public string Name { get; set; }
        public double DistanceFromUser { get; set; } 
        public Location Location { get; set; } 

    }

    public ObservableCollection<GymDistanceInfo> GymsCollection { get; } = new ObservableCollection<GymDistanceInfo>();


    public GymLocatorViewModel()
    {
        LocateGymsCommand = new Command(async () => await LocateGyms());

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
                await NavigationHelper.HandleSingleSwipe("///CalorieCalculator");
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
                await NavigationHelper.HandleSingleSwipe("///AIFitnessCoach");
                LayoutOpacity = 1.0;

            }
        });

    }

    public void MoveToSelectedGym(GymDistanceInfo selectedGym)
    {
        MessagingCenter.Send(this, "UpdateMapRegion", selectedGym.Location);
    }

    private async Task<List<(string Name, Location Location)>> FetchGymsAsync(Location location)
    {
        var gyms = new List<(string Name, Location Location)>();
        string apiKey = Environment.GetEnvironmentVariable("MAPSAPIKEY");
        string requestUri = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={location.Latitude},{location.Longitude}&radius=1500&type=gym&key={apiKey}";

        try
        {
            var response = await _httpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);
                var results = jsonDoc.RootElement.GetProperty("results");

                foreach (var result in results.EnumerateArray())
                {
                    var name = result.GetProperty("name").GetString();
                    var lat = result.GetProperty("geometry").GetProperty("location").GetProperty("lat").GetDouble();
                    var lng = result.GetProperty("geometry").GetProperty("location").GetProperty("lng").GetDouble();

                    if (name != null)
                    {
                        gyms.Add((name, new Location(lat, lng)));
                    }
                }
            }
        }
        catch (Exception)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Error moving to selected gym, please try again", "Close");
        }

        return gyms;
    }

    public void MoveToGymLocation(GymDistanceInfo selectedGym)
    {
        MessagingCenter.Send(this, "UpdateMapRegion", selectedGym.Location);
    }
   
    private async Task LocateGyms()
    {

        var hasInternetPermission = await PermissionsAndInternetHandlers.CheckInternetStatus();
        if (hasInternetPermission != true)
        {
            return;
        }

        var hasPermission = await PermissionsAndInternetHandlers.CheckAndRequestLocationPermission();
        if (hasPermission)
        {
            ButtonTextColor = Color.FromRgb(255,255,255);
            IsMapVisible = true;
            ButtonText = "Locating Gyms...";
            ButtonColor = Color.FromRgb(25, 25, 112);

            IsUserLocationVisible = true;

            var currentUserLocation = await GetCurrentUserLocationAsync();
            if (currentUserLocation != null)
            {
                var gyms = await FetchGymsAsync(currentUserLocation);
                if (gyms.Any())
                {
                    MessagingCenter.Send(this, MapMessages.UpdateMapRegionAndAddPin, (currentUserLocation, Distance.FromMiles(1), "You are here", PinType.Generic));

                    foreach (var gym in gyms)
                    {
                        var gymLocation = gym.Location;
                        var gymLabel = gym.Name;
                        MessagingCenter.Send(this, MapMessages.UpdateMapRegionAndAddPin, (gymLocation, Distance.FromMiles(0.5), gymLabel, PinType.Place));
                    }

                    var gymDistances = gyms.Select(gym => new GymDistanceInfo
                    {
                        Name = gym.Name,
                        DistanceFromUser = CalculateDistance(currentUserLocation, gym.Location),
                        Location = gym.Location
                    })
                    .OrderBy(g => g.DistanceFromUser)
                    .ToList();

                    GymsCollection.Clear();

                    foreach (var gymDistance in gymDistances)
                    {
                        GymsCollection.Add(gymDistance);
                    }
                    ButtonTextColor = Color.FromRgb(0, 0, 0);
                    ButtonText = "Nearby Gyms Found!";
                    VibrationHelper.Vibrate();
                    ButtonColor = Color.FromRgb(0, 255, 0);

                }
                else
                {
                    ButtonText = "No gyms found in the nearby vicinity.";
                    ButtonColor = Color.FromRgb(255, 0, 0);
                }
            }
            else
            {
                ButtonText = "Error Encountered with getting your location, please try again later.";
                ButtonColor = Color.FromRgb(255, 0, 0);
            }
        }
        else
        {
            ButtonText = "Locate Gyms Nearby!";
            IsMapVisible = false;
            return;
        }
    }

    private double CalculateDistance(Location userLocation, Location gymLocation)
    {
        double R = 6371; 
        double latDistance = ToRadians(gymLocation.Latitude - userLocation.Latitude);
        double lonDistance = ToRadians(gymLocation.Longitude - userLocation.Longitude);
        double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2) +
                   Math.Cos(ToRadians(userLocation.Latitude)) * Math.Cos(ToRadians(gymLocation.Latitude)) *
                   Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = R * c;

        return distance;
    }

    private double ToRadians(double angleInDegrees)
    {
        return angleInDegrees * (Math.PI / 180);
    }


    private async Task<Location> GetCurrentUserLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium);
            var location = await Geolocation.GetLocationAsync(request);
            if (location != null)
            {
                return location;
            }
            else
            {
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

}