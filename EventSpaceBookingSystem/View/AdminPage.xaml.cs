using EventSpaceBookingSystem.ViewModel;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using Microsoft.Maui.Controls;
using Mopups.Services;

namespace EventSpaceBookingSystem.View;

public partial class AdminPage : ContentPage
{
    public AdminPage()
    {
        InitializeComponent();
        this.BindingContext = new AdminPageViewModel();
        LoadOwners();
    }

    private async void LoadOwners()
    {
        var owners = await AdminFileService.LoadAllOwnersAsync();
        OwnersListView.ItemsSource = owners; // OwnersListView: your ListView/CollectionView name
    }

    // Button click handlers for filter controls
    private void OnAllOwnersClicked(object sender, EventArgs e)
    {
        if (BindingContext is AdminPageViewModel viewModel)
            viewModel.FilterOwners("All");
    }

    private void OnActiveOwnersClicked(object sender, EventArgs e)
    {
        if (BindingContext is AdminPageViewModel viewModel)
            viewModel.FilterOwners("Active");
    }

    private void OnDeactivatedOwnersClicked(object sender, EventArgs e)
    {
        if (BindingContext is AdminPageViewModel viewModel)
            viewModel.FilterOwners("Deactivated");
    }

    private void OnSelectDatesClicked(object sender, EventArgs e)
    {
        if (BindingContext is AdminPageViewModel viewModel)
            viewModel.SelectDateRangeCommand.Execute(null);
    }

    private void OnSelectLocationClicked(object sender, EventArgs e)
    {
        if (BindingContext is AdminPageViewModel viewModel)
            viewModel.SelectLocationCommand.Execute(null);
    }

    private void OnSelectOwnerClicked(object sender, EventArgs e)
    {
        if (BindingContext is AdminPageViewModel viewModel)
            viewModel.SelectOwnerCommand.Execute(null);
    }

    private void OnClearFiltersClicked(object sender, EventArgs e)
    {
        if (BindingContext is AdminPageViewModel vm)
        {
            vm.StartDate = null;
            vm.EndDate = null;
            vm.SelectedLocation = null;
            vm.SelectedOwner = null;
            vm.ApplyTransactionFilters();
        }
    }

    public void OnProfileIconClicked(object sender, EventArgs e)
    {
        MopupService.Instance.PushAsync(new ProfileBox("admin"));
    }

}
