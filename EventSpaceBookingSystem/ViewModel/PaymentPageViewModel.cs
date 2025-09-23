using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using EventSpaceBookingSystem.View;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.ViewModel
{
    public class PaymentPageViewModel : INotifyPropertyChanged
    {
        public string Username { get; set; }
        public string Email { get; set; }

        public Booking Booking { get; }

        public string BookingSummary =>
            $"Booking ID: {Booking.BookingId}\n" +
            $"Venue: {Booking.Headline}\n" +
            $"Booking Date: {Booking.BookingDate:MMMM dd, yyyy}\n" +
            $"Guests: {Booking.GuestCount}\n" +
            $"Time: {Booking.EventTime}\n" +
            $"Extra Chairs: {Booking.ExtraChairs}\n" +
            $"Time Extension: {Booking.TimeExtension}\n" +
            $"\t\t\tTotal: ₱{Booking.Price:N2}";

        public ObservableCollection<string> PaymentMethods { get; set; } = new() { "E-Wallet", "Credit/Debit" };
        public ObservableCollection<string> WalletProviders { get; set; } = new() { "GCash", "Maya" };

        private string _selectedPaymentMethod;
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                if (_selectedPaymentMethod != value)
                {
                    _selectedPaymentMethod = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsEWallet));
                    OnPropertyChanged(nameof(IsCard));
                }
            }
        }

        // E-Wallet Fields
        private string _walletProvider;
        public string WalletProvider
        {
            get => _walletProvider;
            set { _walletProvider = value; OnPropertyChanged(); }
        }

        private string _walletId;
        public string WalletId
        {
            get => _walletId;
            set { _walletId = value; OnPropertyChanged(); }
        }

        // Card Fields
        private string _cardNumber;
        public string CardNumber
        {
            get => _cardNumber;
            set { _cardNumber = value; OnPropertyChanged(); }
        }

        private string _expiry;
        public string Expiry
        {
            get => _expiry;
            set { _expiry = value; OnPropertyChanged(); }
        }

        private string _cvv;
        public string CVV
        {
            get => _cvv;
            set { _cvv = value; OnPropertyChanged(); }
        }
        public ICommand CancelPaymentCommand { get; }
        public bool IsEWallet => SelectedPaymentMethod == "E-Wallet";
        public bool IsCard => SelectedPaymentMethod == "Credit/Debit";

        public ICommand SubmitPaymentCommand { get; }

        public PaymentPageViewModel(string username, string email, Booking booking)
        {
            Username = username;
            Email = email;
            Booking = booking;

            WalletProvider = WalletProviders[0]; // Default selection
            SelectedPaymentMethod = "E-Wallet";

            SubmitPaymentCommand = new Command(OnSubmitPayment);
            CancelPaymentCommand = new Command(OnCancelPayment);
        }
        private async void OnCancelPayment()
        {
            // Navigate back to CartPage with username and email
            await Application.Current.MainPage.Navigation.PushAsync(new CartPage(Username, Email));
        }

        private async void OnSubmitPayment()
        {
            // Validate required fields based on payment method
            if (IsEWallet)
            {
                if (string.IsNullOrWhiteSpace(WalletProvider) || string.IsNullOrWhiteSpace(WalletId))
                {
                    await Application.Current.MainPage.DisplayAlert("Missing Info", "Please complete all E-Wallet fields.", "OK");
                    return;
                }
            }
            else if (IsCard)
            {
                if (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(Expiry) || string.IsNullOrWhiteSpace(CVV))
                {
                    await Application.Current.MainPage.DisplayAlert("Missing Info", "Please complete all Credit/Debit fields.", "OK");
                    return;
                }
            }

            // ✅ Update booking status
            Booking.Status = "Payment Successful";

            // ✅ Save updated booking to UserFileService
            await UserFileService.UpdateBookingStatusAsync(Username, Booking.BookingId, "Payment Successful");

            // ✅ Notify event space owner
            int ownerId = await UserFileService.FindOwnerIdByEventTitleAsync(Booking.Headline);
            if (ownerId > 0)
            {
                SpaceOwnerFileService.AddNotificationByOwnerId(
                    ownerId,
                    "Payment Received",
                    $"{Username} has successfully paid for their booking '{Booking.Headline}' (Booking ID: {Booking.BookingId})."
                );
            }

            // ✅ Notify user
            await Application.Current.MainPage.DisplayAlert(
                "Payment Successful",
                $"Thank you! Your payment for '{Booking.Headline}' has been received.",
                "OK"
            );

            // Redirect
            await Application.Current.MainPage.Navigation.PushAsync(new View.CartPage(Username, Email));
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
