using EventSpaceBookingSystem.View;

namespace EventSpaceBookingSystem
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("venuepage", typeof(VenuePage));

        }
    }
}
