using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using EventSpaceBookingSystem.ViewModel;
using Microsoft.Maui.Controls;
using Mopups.Services;
using System;

namespace EventSpaceBookingSystem.View
{
    public partial class OwnerUpdateSpacePage : ContentPage
    {
        private readonly OwnerUpdateSpaceViewModel viewModel;
        private readonly int _ownerId;

        public OwnerUpdateSpacePage(EventModel selectedSpace, int ownerId)
        {
            InitializeComponent();
            _ownerId = ownerId;

            viewModel = new OwnerUpdateSpaceViewModel(selectedSpace, _ownerId);
            BindingContext = viewModel;

            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void OnUpdateBtnClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirm", "Are you sure you want to update this space?", "Yes", "No");
            if (!confirm) return;

            await viewModel.SaveChangesAsync();
            await DisplayAlert("Updated", "Event space updated successfully!", "OK");
            await Navigation.PopAsync(); // Go back to OwnerHomePage
        }

        private async void OnCancelBtnClicked(object sender, EventArgs e)
        {
            bool cancel = await DisplayAlert("Cancel", "Discard changes?", "Yes", "No");
            if (cancel)
            {
                await Navigation.PopAsync();
            }
        }

        private void OnNotificationIconClicked(object sender, EventArgs e)
        {
            MopupService.Instance.PushAsync(new NotifBox(_ownerId));
        }

        private void OnProfileIconClicked(object sender, EventArgs e)
        {
            MopupService.Instance.PushAsync(new ProfileBox(_ownerId));
        }

        private void OnPencilIconClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton pencil)
            {
                var parent = pencil.Parent;
                while (parent is not null && parent is not StackLayout)
                    parent = (parent as VisualElement)?.Parent;

                if (parent is StackLayout stack)
                {
                    var input = stack.Children.FirstOrDefault(v => v is Entry || v is Editor);
                    switch (input)
                    {
                        case Entry entry:
                            entry.IsReadOnly = false;
                            entry.Focus();
                            break;
                        case Editor editor:
                            editor.IsReadOnly = false;
                            editor.Focus();
                            break;
                    }
                }
            }
        }

    }
}
