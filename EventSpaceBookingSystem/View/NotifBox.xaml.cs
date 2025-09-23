using Mopups.Services;
using System;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.View
{
    public partial class NotifBox : Mopups.Pages.PopupPage
    {
        public NotifBox(int userId)
        {
            InitializeComponent();
            BindingContext = new ViewModel.NotifBoxViewModel(userId);
        }
    }
}
