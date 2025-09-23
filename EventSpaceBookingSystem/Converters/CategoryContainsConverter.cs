using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.Converters
{
    public class CategoryContainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 'value' is the current category string
            // Get ViewModel from MainPage
            if (Application.Current.MainPage.BindingContext is ViewModel.EventSpaceOwnerJoinViewModel vm &&
                value is string category)
            {
                return vm.SelectedCategories.Contains(category);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
