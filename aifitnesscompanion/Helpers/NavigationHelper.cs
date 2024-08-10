namespace aifitnesscompanion.Helpers
{
    public static class NavigationHelper
    {
       
        public static async Task HandleSingleSwipe(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                throw new ArgumentException("Route must not be null or empty.");
            }
            await Shell.Current.GoToAsync($"///{route}");
        }
    }
    
}
