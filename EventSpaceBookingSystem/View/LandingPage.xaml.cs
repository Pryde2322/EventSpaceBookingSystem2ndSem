using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.ViewModel;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.View
{
    public partial class LandingPage : ContentPage
    {
        private readonly LandingPageViewModel _viewModel;

        public LandingPage()
        {
            InitializeComponent();
            _viewModel = new LandingPageViewModel();
            BindingContext = _viewModel;

       
        }

        private async Task InitializeBookingFilesForStandardUsers()
        {
            await UserFileService.EnsureAllStandardUserBookingFilesAsync();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }

        private void OnSearchClicked(object sender, EventArgs e)
        {
            // Trigger the SearchCommand in the ViewModel when Search Button is clicked
            if (_viewModel.SearchCommand.CanExecute(null))
            {
                _viewModel.SearchCommand.Execute(null);
            }

            DisplayAlert("Search", $"Searching for '{_viewModel.SearchQuery}' in '{_viewModel.LocationQuery}'", "OK");
        }

        private void OnEventSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is EventModel eventVenue)
            {
                DisplayAlert("Event Selected", $"You selected {eventVenue.Title}", "OK");
            }
        }

        private void ScrollToCurrentIndex()
        {
            if (eventsCollection != null && _viewModel.CurrentEvent != null)
            {
                eventsCollection.ScrollTo(_viewModel.CurrentEvent, position: ScrollToPosition.Center, animate: true);
            }
        }

        private void OnPreviousCommand(object sender, EventArgs e)
        {
            _viewModel.PreviousCommand.Execute(null);
            ScrollToCurrentIndex();
        }

        private void OnNextCommand(object sender, EventArgs e)
        {
            _viewModel.NextCommand.Execute(null);
            ScrollToCurrentIndex();
        }

    }
}
