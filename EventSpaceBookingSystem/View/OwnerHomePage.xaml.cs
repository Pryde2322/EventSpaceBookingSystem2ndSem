using System;
using EventSpaceBookingSystem.Model;
using Mopups.Services;
using EventSpaceBookingSystem.ViewModel;
using EventSpaceBookingSystem.Services;
using EventSpaceBookingSystem.Helpers; // For Session

namespace EventSpaceBookingSystem.View
{
    public partial class OwnerHomePage : ContentPage
    {
        private OwnerHomePageViewModel ownerViewModel;
        private EventModel _selectedSpace;
        private int _ownerId;

        // ✅ Constructor 1: with ownerId passed explicitly
        public OwnerHomePage(int ownerId)
        {
            InitializeComponent();
            _ownerId = ownerId;
            Session.CurrentOwnerId = ownerId; // Optional: update session as fallback
            Init();
        }

        // ✅ Constructor 2: using Session (for nav that doesn’t pass Id directly)
        public OwnerHomePage()
        {
            InitializeComponent();
            _ownerId = Session.CurrentOwnerId;
            Init();
        }

        private void Init()
        {
            ownerViewModel = new OwnerHomePageViewModel(_ownerId);
            BindingContext = ownerViewModel;
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);
        }

        // Changed second parameter to EventArgs to match Button.Clicked signature
        public async void OnUpdateBtnClicked(object sender, EventArgs e)
        {
            if (_selectedSpace != null)
            {
                await Navigation.PushAsync(new OwnerUpdateSpacePage(_selectedSpace, _ownerId));
            }
            else
            {
                await DisplayAlert("Selection Required", "Please select an event space to update.", "OK");
            }
        }

        // Changed second parameter to EventArgs and made async to await the PushAsync call
        public async void OnAddEventBtnClicked(object sender, EventArgs e)
        {
            await MopupService.Instance.PushAsync(new AddEventSpaceBox(ownerViewModel, _ownerId));
        }

        private void OnEventSpaceSelected(object sender, SelectionChangedEventArgs e)
        {
            _selectedSpace = e.CurrentSelection.FirstOrDefault() as EventModel;
        }

        // Handle checkbox changed to enforce single selection
        private void OnItemCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.BindingContext is EventModel changed)
            {
                if (e.Value)
                {
                    // Uncheck others
                    foreach (var item in ownerViewModel.EventSpaces)
                    {
                        if (!ReferenceEquals(item, changed) && item.IsSelected)
                            item.IsSelected = false;
                    }
                    _selectedSpace = changed;
                }
                else
                {
                    // If unchecked the selected item, clear selection
                    if (ReferenceEquals(_selectedSpace, changed))
                        _selectedSpace = null;
                }
            }
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
