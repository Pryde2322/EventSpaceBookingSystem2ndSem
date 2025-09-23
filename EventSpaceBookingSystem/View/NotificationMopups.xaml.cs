using Mopups.Services;
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace EventSpaceBookingSystem
{
    public partial class NotificationMopups : Mopups.Pages.PopupPage
    {
        public NotificationMopups(int userId)
        {
            InitializeComponent();
            BindingContext = new ViewModel.NotificationMopupsViewModel(userId);
        }


        private async void OnBackgroundTapped(object sender, EventArgs e)
        {
            await MopupService.Instance.RemovePageAsync(this);
        }
    }
}
