using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using EventSpaceBookingSystem.ViewModel;

namespace EventSpaceBookingSystem.Converters
{
    public class CategoryCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is EventSpaceOwnerJoinViewModel vm && value is string category)
            {
                return vm.SelectedCategories.Contains(category);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is EventSpaceOwnerJoinViewModel vm && value is bool isChecked && BindingContextHack.CurrentCategory is string category)
            {
                if (isChecked)
                {
                    if (!vm.SelectedCategories.Contains(category) && vm.SelectedCategories.Count < 3)
                        vm.SelectedCategories.Add(category);
                }
                else
                {
                    vm.SelectedCategories.Remove(category);
                }
            }

            return null;
        }
    }

    // Workaround for parameter limitation in ConvertBack (optional helper)
    public static class BindingContextHack
    {
        public static object CurrentCategory { get; set; }
    }
}
