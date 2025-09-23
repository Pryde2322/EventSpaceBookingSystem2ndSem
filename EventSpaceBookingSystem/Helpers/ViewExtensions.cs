using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Threading.Tasks;

namespace EventSpaceBookingSystem.Helpers
{
    public static class ViewExtensions
    {
        public static async Task<Point?> GetAbsoluteLocationAsync(this VisualElement view)
        {
            if (view == null || !view.IsVisible || view.Handler == null)
                return null;

            await Task.Delay(50); // Ensure layout is rendered

            try
            {
                var parent = view;
                double x = view.X;
                double y = view.Y;

                while (parent.Parent is VisualElement parentView)
                {
                    x += parentView.X;
                    y += parentView.Y;
                    parent = parentView;
                }

                return new Point(x, y);
            }
            catch
            {
                return null;
            }
        }
    }
}
