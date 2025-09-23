using Mopups.Services;
using EventSpaceBookingSystem.ViewModel;

namespace EventSpaceBookingSystem.View;

public partial class OwnerProfilePage : ContentPage
{
    private readonly OwnerHomePageViewModel ownerViewModel;
    private readonly int _ownerId;

    public OwnerProfilePage(int ownerId)
    {
        InitializeComponent();
        _ownerId = ownerId;

        ownerViewModel = new OwnerHomePageViewModel(_ownerId);

        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetHasBackButton(this, false);
    }

    public void OnAddEventBtnClicked(object sender, TappedEventArgs e)
    {
        // ✅ Pass both the ViewModel and ownerId to the popup
        MopupService.Instance.PushAsync(new AddEventSpaceBox(ownerViewModel, _ownerId));
    }
}
