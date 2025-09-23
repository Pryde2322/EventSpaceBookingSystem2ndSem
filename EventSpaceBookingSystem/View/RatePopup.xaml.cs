using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.View
{
    public partial class RatePopup : Popup
    {
        public int SelectedRating { get; private set; } = 0;

        public RatePopup()
        {
            InitializeComponent();
        }

        private void OnOkClicked(object sender, EventArgs e)
        {
            if (RatingPicker.SelectedIndex >= 0)
            {
                SelectedRating = (int)RatingPicker.SelectedItem;
                Close(SelectedRating);
            }
            else
            {
                Close(0); // no selection
            }
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close(0); // canceled
        }
    }
}
