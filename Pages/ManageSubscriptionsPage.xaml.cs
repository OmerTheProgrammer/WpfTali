using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model;
using Service;

namespace WpfTali.Pages
{
    public partial class ManageSubscriptionsPage : Page
    {
        private readonly Apiservice _apiService = new Apiservice();
        private Subscription _selectedSub;

        public ManageSubscriptionsPage()
        {
            InitializeComponent();
            LoadSubscriptions();
        }

        private async void LoadSubscriptions()
        {
            try
            {
                var subs = await _apiService.GetAllSubscription();
                SubsDataGrid.ItemsSource = subs?.Cast<Subscription>().ToList();
            }
            catch (Exception ex) { MessageBox.Show($"שגיאה בטעינת מנויים: {ex.Message}"); }
        }

        private void SubsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SubsDataGrid.SelectedItem is Subscription sub)
            {
                _selectedSub = sub;
                TxtSubName.Text = sub.Name_of_sub;
                TxtSubPrice.Text = sub.Price.ToString();
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TxtSubPrice.Text, out double subPrice))
            {
                MessageBox.Show("נא להזין מחיר תקין");
                return;
            }

            var newSub = new Subscription
            {
                Name_of_sub = TxtSubName.Text,
                Price = subPrice
            };

            if (await _apiService.InsertASubscription(newSub) > 0)
            {
                MessageBox.Show("סוג מנוי נוסף בהצלחה!");
                LoadSubscriptions();
                ClearFields();
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.GoBack();
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSub == null) return;
            if (!double.TryParse(TxtSubPrice.Text, out double subPrice)) return;

            _selectedSub.Name_of_sub = TxtSubName.Text;
            _selectedSub.Price = subPrice;

            if (await _apiService.UpdateASubscription(_selectedSub) > 0)
            {
                MessageBox.Show("המנוי עודכן!");
                LoadSubscriptions();
                ClearFields();
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSub == null) return;

            var res = MessageBox.Show("האם למחוק סוג מנוי זה?", "אישור", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                if (await _apiService.DeleteASubscription(_selectedSub.Id) > 0)
                {
                    MessageBox.Show("נמחק בהצלחה!");
                    LoadSubscriptions();
                    ClearFields();
                }
            }
        }

        private void ClearFields()
        {
            _selectedSub = null;
            TxtSubName.Clear();
            TxtSubPrice.Clear();
            SubsDataGrid.SelectedItem = null;
        }
    }
}
