using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EventSpaceBookingSystem.Model;
using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.ViewModel
{
    public class AdminPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Owner> Owners { get; set; } = new();
        private ObservableCollection<Owner> AllOwners { get; set; } = new();

        public ObservableCollection<Transaction> Transactions { get; set; } = new();

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SelectedLocation { get; set; }
        public string SelectedOwner { get; set; }

        private ObservableCollection<Transaction> allTransactions = new();

        public ICommand SelectDateRangeCommand { get; }
        public ICommand SelectLocationCommand { get; }
        public ICommand SelectOwnerCommand { get; }

        public AdminPageViewModel()
        {
            LoadOwnersFromFile();
            LoadTransactionsFromFile();

            SelectDateRangeCommand = new Command(OnSelectDates);
            SelectLocationCommand = new Command(OnSelectLocation);
            SelectOwnerCommand = new Command(OnSelectOwner);
        }

        private async void LoadOwnersFromFile()
        {
            var list = await AdminFileService.LoadAllOwnersAsync();

            foreach (var owner in list)
            {
                // ✅ Set IsActive based on saved RaMenCoStatus before binding
                if (!string.IsNullOrWhiteSpace(owner.RaMenCoStatus))
                {
                    owner.IsActive = owner.RaMenCoStatus == "Activated";
                }

                // 🔁 Watch for toggle change
                owner.PropertyChanged += async (s, e) =>
                {
                    if (e.PropertyName == nameof(owner.IsActive))
                    {
                        if (!string.IsNullOrWhiteSpace(owner.RaMenCoStatus))
                        {
                            owner.RaMenCoStatus = owner.IsActive ? "Activated" : "Deactivated";
                            await AdminFileService.UpdateOwnerStatusAsync(owner.Name, owner.RaMenCoStatus);
                        }
                    }
                };
            }

            AllOwners = new ObservableCollection<Owner>(list);
            Owners = new ObservableCollection<Owner>(list);
            OnPropertyChanged(nameof(Owners));
        }


        private async void LoadTransactionsFromFile()
        {
            var list = await AdminFileService.LoadAllTransactionsFromBookingsAsync();
            allTransactions = new ObservableCollection<Transaction>(list);
            Transactions = new ObservableCollection<Transaction>(list);
            OnPropertyChanged(nameof(Transactions));
        }

        public void ApplyTransactionFilters()
        {
            var filtered = allTransactions.AsEnumerable();

            if (StartDate.HasValue && EndDate.HasValue)
            {
                filtered = filtered.Where(t =>
                {
                    DateTime.TryParseExact(t.Date, "MMM dd, yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsed);
                    return parsed >= StartDate.Value && parsed <= EndDate.Value;
                });
            }

            if (!string.IsNullOrEmpty(SelectedLocation))
            {
                filtered = filtered.Where(t => t.Space.Contains(SelectedLocation, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(SelectedOwner))
            {
                filtered = filtered.Where(t => t.Owner == SelectedOwner);
            }

            Transactions = new ObservableCollection<Transaction>(filtered);
            OnPropertyChanged(nameof(Transactions));
        }

        public void FilterOwners(string status)
        {
            if (status == "All")
            {
                Owners = new ObservableCollection<Owner>(AllOwners);
            }
            else if (status == "Active")
            {
                Owners = new ObservableCollection<Owner>(AllOwners.Where(o => o.IsActive));
            }
            else if (status == "Deactivated")
            {
                Owners = new ObservableCollection<Owner>(AllOwners.Where(o => !o.IsActive));
            }

            OnPropertyChanged(nameof(Owners));
        }

        private async void OnSelectDates()
        {
            var start = await Application.Current.MainPage.DisplayPromptAsync("Start Date", "Enter start date (yyyy-MM-dd)");
            var end = await Application.Current.MainPage.DisplayPromptAsync("End Date", "Enter end date (yyyy-MM-dd)");

            if (DateTime.TryParse(start, out DateTime startDate) && DateTime.TryParse(end, out DateTime endDate))
            {
                StartDate = startDate;
                EndDate = endDate;
                ApplyTransactionFilters();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Invalid", "Please enter valid dates.", "OK");
            }
        }

        private async void OnSelectLocation()
        {
            var location = await Application.Current.MainPage.DisplayPromptAsync("Location Filter", "Enter venue name or keyword:");
            if (!string.IsNullOrWhiteSpace(location))
            {
                SelectedLocation = location;
                ApplyTransactionFilters();
            }
        }

        private async void OnSelectOwner()
        {
            var owner = await Application.Current.MainPage.DisplayPromptAsync("Owner Filter", "Enter Owner ID (e.g., EO-1):");
            if (!string.IsNullOrWhiteSpace(owner))
            {
                SelectedOwner = owner;
                ApplyTransactionFilters();
            }
        }

        // 🔧 Fix: INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
