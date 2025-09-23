using Mopups.Services;

namespace EventSpaceBookingSystem.View;

public partial class OwnerSettingsPage : ContentPage
{
    private int _ownerId;

    public OwnerSettingsPage(int ownerId)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetHasBackButton(this, false);

        _ownerId = ownerId; // ✅ Assign incoming owner ID
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
