using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.View;
using EventSpaceBookingSystem.ViewModel;



namespace EventSpaceBookingSystem
{
    public partial class App : Application
    {
        private readonly ExplorePageViewModel _viewModel;
        public App()
        {
            InitializeComponent();
            //MainPage = new NavigationPage(new LandingPage());
            //MainPage = new NavigationPage(new OwnerHomePage());
            //MainPage = new NavigationPage(new OwnerProfilePage());
            MainPage = new NavigationPage(new SplashScreen());
        }
    }
}
