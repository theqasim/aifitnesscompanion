using System.Diagnostics;

namespace aifitnesscompanion.Helpers;

public static class PermissionsAndInternetHandlers
{

    public static async Task<bool> CheckAndRequestMicrophonePermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
                bool goToSettingsPage = await Application.Current.MainPage.DisplayAlert(
                    "Microphone Permission Required",
                    "Microphone permission is needed to use this feature. Please go to Settings and enable the microphone permission for this app.",
                    "Go to Settings",
                    "Cancel");

                if (goToSettingsPage)
                {
                    AppInfo.ShowSettingsUI();
                }
                return false; 
            
        }
        return true; 
    }

    public static async Task<bool> CheckInternetStatus()
    {
        var current = Connectivity.NetworkAccess;
        if (current != NetworkAccess.Internet)
        {
            bool goToSettingsPage = await Application.Current.MainPage.DisplayAlert(
                "You need internet access!",
                "To use this feature, internet access is required. Please go to Settings and enable internet access for this app.",
                "Go to Settings",
                "Cancel");

            if (goToSettingsPage)
            {
                AppInfo.ShowSettingsUI();
            }

            return false;
        }

        return true;
    }

    public static async Task<bool> CheckAndRequestLocationPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                bool goToSettings = await Application.Current.MainPage.DisplayAlert(
                    "Location Permission Required",
                    "Location permission is needed to use the Locate Gyms feature. Please go to Settings and enable location permission for this app.",
                    "Go to Settings",
                    "Cancel");

                if (goToSettings)
                {
                    AppInfo.ShowSettingsUI();
                }
                return false;
            }
        }
        else
        {
            return status == PermissionStatus.Granted;
        }
        return status == PermissionStatus.Granted;
    }

    public static async Task<bool> CheckAndRequestCameraPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {

                bool goToSettings = await Application.Current.MainPage.DisplayAlert(
                    "Camera Permission Required",
                    "Camera permission is needed to use this feature. Please go to Settings and enable the camera permission for this app.",
                    "Go to Settings",
                    "Cancel");

                if (goToSettings)
                {
                    AppInfo.ShowSettingsUI();
                }

                return false; 
            }
        }
        return true; 
    }

    public static async Task<bool> CheckAndRequestStoragePermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                bool goToSettings = await Application.Current.MainPage.DisplayAlert(
                    "Storage Permission Required",
                    "Storage permission is needed to use this feature. Please go to Settings and enable the storage permission for this app.",
                    "Go to Settings",
                    "Cancel");

                if (goToSettings)
                {
                    AppInfo.ShowSettingsUI(); 
                }

                return false; 
            }
        }
        return true; 
    }

}
