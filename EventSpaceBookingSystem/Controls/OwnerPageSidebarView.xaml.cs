using EventSpaceBookingSystem.View;
using EventSpaceBookingSystem.ViewModel;

namespace EventSpaceBookingSystem.Controls;

public partial class OwnerPageSidebarView : ContentView
{
    public enum SidebarPage
    {
        Home,
        Profile,
        Settings,
        Edit
    }

    public static readonly BindableProperty SelectedPageProperty =
    BindableProperty.Create(nameof(SelectedPage), typeof(SidebarPage), typeof(OwnerPageSidebarView), SidebarPage.Home);

    public SidebarPage SelectedPage
    {
        get => (SidebarPage)GetValue(SelectedPageProperty);
        set => SetValue(SelectedPageProperty, value);
    }
    public OwnerPageSidebarView() // ← ✅ Required for XAML
    {
        InitializeComponent();
    }


    private int _ownerId;
    private string _ownerName;
    public OwnerPageSidebarView(int ownerId, string ownerName)
    {
        InitializeComponent();
        _ownerId = ownerId;
        _ownerName = ownerName;
    }

    public async void OnHomeClicked(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new OwnerHomePage(), false);
    }
    
    //public async void OnProfileClicked(object sender, TappedEventArgs e)
    //{
    //    await Navigation.PushAsync(new OwnerProfilePage(_ownerId), false);
    //}

    //public async void OnSettingsClicked(object sender, TappedEventArgs e) 
    //{
    //    await Navigation.PushAsync(new OwnerSettingsPage(_ownerId), false);
    //}

    public async void OnEditClicked(object sender, TappedEventArgs e) 
    {
        await Navigation.PushAsync(new OwnerBookingInfoPage(), false);
    }
}