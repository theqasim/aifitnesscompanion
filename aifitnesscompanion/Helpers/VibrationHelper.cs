namespace aifitnesscompanion.Helpers;


public static class VibrationHelper
{
    public static void Vibrate(TimeSpan? duration = null)
    {
        try
        {
            var vibrationDuration = duration ?? TimeSpan.FromSeconds(0.5); 
            Vibration.Vibrate(vibrationDuration);
        }
        catch (FeatureNotSupportedException fnsEx)
        {
        }
        catch (Exception ex)
        {
        }
    }

}
