using EventSpaceBookingSystem.ViewModel;
using EventSpaceBookingSystem.Helpers;
using Mopups.Services;

namespace EventSpaceBookingSystem.View
{
    public partial class OwnerBookingInfoPage : ContentPage
    {
        private readonly int _ownerId;
        public OwnerBookingInfoPage()
        {
            InitializeComponent();
            int ownerId = Session.CurrentOwnerId; // Get the current owner ID from the session
            // Use Session.CurrentOwnerId inside the ViewModel, no need to pass it here
            BindingContext = new OwnerBookingInfoPageViewModel(ownerId);
            _ownerId = ownerId;
            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
        }

        public void OnNotificationIconClicked(object sender, EventArgs e)
        {
            MopupService.Instance.PushAsync(new NotifBox(_ownerId));
        }

        public void OnProfileIconClicked(object sender, EventArgs e)
        {
            MopupService.Instance.PushAsync(new ProfileBox(_ownerId));
        }
    }
}
