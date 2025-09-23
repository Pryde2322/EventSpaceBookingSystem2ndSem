using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using EventSpaceBookingSystem.ViewModel;
using Mopups.Services;
using System;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.View;

public partial class AddEventSpaceBox : Mopups.Pages.PopupPage
{
    private readonly OwnerHomePageViewModel _ownerViewModel;
    private readonly int _ownerId;
    private readonly AddEventSpaceBoxViewModel _localViewModel;

    public AddEventSpaceBox(OwnerHomePageViewModel viewModel, int ownerId)
    {
        InitializeComponent();
        _ownerViewModel = viewModel;
        _ownerId = ownerId;

        _localViewModel = new AddEventSpaceBoxViewModel(_ownerViewModel, _ownerId);
        BindingContext = _localViewModel;
        this.BackgroundClicked += async (s, e) => await Mopups.Services.MopupService.Instance.PopAsync();

    }

    protected override bool OnBackgroundClicked()
    {
        // ✅ Close popup when background is tapped
        MopupService.Instance.PopAsync();
        return true;
    }
}
