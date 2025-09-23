using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.ViewModel;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.View;

public partial class SplashScreen : ContentPage
{
    public SplashScreen()
    {
        InitializeComponent();
        StartLoading();
        //  Call method to ensure standard booking files exist
        _ = InitializeBookingFilesForStandardUsers();
    }

    private async void StartLoading()
    {
        await Task.Delay(2000);

        await this.FadeTo(0, 400);

        Application.Current.MainPage = new NavigationPage(new LandingPage());
    }

  private async Task InitializeBookingFilesForStandardUsers()
    {
        await UserFileService.EnsureAllStandardUserBookingFilesAsync();
    } 
}
