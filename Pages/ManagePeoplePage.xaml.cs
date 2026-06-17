//using System;
//using System.Linq;
//using System.Windows;
//using System.Windows.Controls;
//using Model;
//using Service;

//namespace WpfTali.Pages
//{
//    public partial class ManagePeoplePage : Page
//    {
//        private readonly Apiservice _apiService = new Apiservice();
//        private Person _selectedPerson;

//        public ManagePeoplePage()
//        {
//            InitializeComponent();
//            LoadPeople();
//        }
//        private async void LoadPeople()
//        {
//            try
//            {
//                var peopleList = await _apiService.GetAllPerson();
//                PeopleDataGrid.ItemsSource = peopleList?.Cast<Person>().ToList();
//            }
//            catch (Exception ex) { MessageBox.Show($"שגיאה בטעינת אנשים: {ex.Message}"); }
//        }
//        private void PeopleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            if (PeopleDataGrid.SelectedItem is Person person)
//            {
//                _selectedPerson = person;
//                TxtNumId.Text = person.Num_id;
//                TxtUserName.Text = person.User_name;
//                TxtFirstName.Text = person.First_name;
//                TxtLastName.Text = person.Last_name;
//                TxtEmail.Text = person.Email;
//                TxtPhone.Text = person.Telephone;
//                TxtPassword.Text = person.Pass;
//                DpBornDate.SelectedDate = person.Born_date;
//                TxtGenderId.Text = person.Id_gender?.Id.ToString() ?? "";
//            }
//        }
//        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                var newPerson = new Person
//                {
//                    Num_id = TxtNumId.Text,
//                    User_name = TxtUserName.Text,
//                    First_name = TxtFirstName.Text,
//                    Last_name = TxtLastName.Text,
//                    Email = TxtEmail.Text,
//                    Telephone = TxtPhone.Text,
//                    Pass = TxtPassword.Text,
//                    Born_date = DpBornDate.SelectedDate ?? DateTime.Today,
//                    Photo = "",
//                    PhotoPath = "",
//                    Id_gender = int.TryParse(TxtGenderId.Text, out int gId)
//                        ? new Gender { Id = gId }
//                        : null
//                };

//                int result = await _apiService.InsertAPerson(newPerson);

//                MessageBox.Show("Result = " + result);

//                if (result > 0)
//                {
//                    MessageBox.Show("המשתמש נוסף בהצלחה");
//                    LoadPeople();
//                    ClearFields();
//                }
//                else
//                {
//                    MessageBox.Show("ההוספה נכשלה");
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.ToString());
//            }
//        }
//        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                if (_selectedPerson == null)
//                {
//                    MessageBox.Show("בחר משתמש לעדכון");
//                    return;
//                }

//                _selectedPerson.Num_id = TxtNumId.Text;
//                _selectedPerson.User_name = TxtUserName.Text;
//                _selectedPerson.First_name = TxtFirstName.Text;
//                _selectedPerson.Last_name = TxtLastName.Text;
//                _selectedPerson.Email = TxtEmail.Text;
//                _selectedPerson.Telephone = TxtPhone.Text;
//                _selectedPerson.Pass = TxtPassword.Text;

//                if (DpBornDate.SelectedDate != null)
//                    _selectedPerson.Born_date = DpBornDate.SelectedDate.Value;

//                if (int.TryParse(TxtGenderId.Text, out int gId))
//                    _selectedPerson.Id_gender = new Gender { Id = gId };

//                MessageBox.Show("ID = " + _selectedPerson.Id);

//                int result = await _apiService.UpdateAPerson(_selectedPerson);

//                MessageBox.Show("Result = " + result);

//                if (result > 0)
//                {
//                    MessageBox.Show("עודכן בהצלחה");
//                    LoadPeople();
//                    ClearFields();
//                }
//                else
//                {
//                    MessageBox.Show("העדכון נכשל");
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.ToString());
//            }
//        }
//        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
//        {
//            if (_selectedPerson == null) return;

//            var res = MessageBox.Show("האם למחוק את המשתמש?", "אישור", MessageBoxButton.YesNo);
//            if (res == MessageBoxResult.Yes)
//            {
//                if (await _apiService.DeleteAPerson(_selectedPerson.Id) > 0)
//                {
//                    MessageBox.Show("נמחק בהצלחה!");
//                    LoadPeople();
//                    ClearFields();
//                }
//            }
//        }
//        private void BtnClear_Click(object sender, RoutedEventArgs e) => ClearFields();
//        private void ClearFields()
//        {
//            _selectedPerson = null;
//            TxtNumId.Clear(); TxtUserName.Clear(); TxtFirstName.Clear(); TxtLastName.Clear();
//            TxtEmail.Clear(); TxtPhone.Clear(); TxtPassword.Clear(); TxtGenderId.Clear();
//            DpBornDate.SelectedDate = null;
//            PeopleDataGrid.SelectedItem = null;
//        }
//        private void BtnBack_Click(object sender, RoutedEventArgs e)
//        {
//            this.NavigationService?.GoBack();
//        }
//    }
//}











using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model;
using Service;

namespace WpfTali.Pages
{
    public partial class ManagePeoplePage : Page
    {
        private readonly Apiservice _apiService = new Apiservice();
        private Person _selectedPerson;

        public ManagePeoplePage()
        {
            InitializeComponent();
            LoadPeople();
        }

        private async void LoadPeople()
        {
            try
            {
                var peopleList = await _apiService.GetAllPerson();
                PeopleDataGrid.ItemsSource = peopleList?.Cast<Person>().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בטעינת אנשים: {ex.Message}");
            }
        }

        private void PeopleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PeopleDataGrid.SelectedItem is Person person)
            {
                _selectedPerson = person;
                TxtNumId.Text = person.Num_id;
                TxtUserName.Text = person.User_name;
                TxtFirstName.Text = person.First_name;
                TxtLastName.Text = person.Last_name;
                TxtEmail.Text = person.Email;
                TxtPhone.Text = person.Telephone;
                TxtPassword.Text = person.Pass;
                DpBornDate.SelectedDate = person.Born_date;
                TxtGenderId.Text = person.Id_gender?.Id.ToString() ?? "";
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newPerson = new Person
                {
                    Num_id = TxtNumId.Text,
                    User_name = TxtUserName.Text,
                    First_name = TxtFirstName.Text,
                    Last_name = TxtLastName.Text,
                    Email = TxtEmail.Text,
                    Telephone = TxtPhone.Text,
                    Pass = TxtPassword.Text,
                    Born_date = DpBornDate.SelectedDate ?? DateTime.Today,
                    Photo = "",
                    PhotoPath = "",
                    Id_gender = int.TryParse(TxtGenderId.Text, out int gId) ? new Gender { Id = gId } : null
                };

                int result = await _apiService.InsertAPerson(newPerson);
                if (result > 0)
                {
                    MessageBox.Show("המשתמש נוסף בהצלחה", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadPeople();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("ההוספה נכשלה", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "שגיאה קריטית");
            }
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedPerson == null)
                {
                    MessageBox.Show("בחר משתמש לעדכון", "תשומת לב", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // Update properties from text fields
                _selectedPerson.Num_id = TxtNumId.Text;
                _selectedPerson.User_name = TxtUserName.Text;
                _selectedPerson.First_name = TxtFirstName.Text;
                _selectedPerson.Last_name = TxtLastName.Text;
                _selectedPerson.Email = TxtEmail.Text;
                _selectedPerson.Telephone = TxtPhone.Text;
                _selectedPerson.Pass = TxtPassword.Text;

                if (DpBornDate.SelectedDate != null)
                    _selectedPerson.Born_date = DpBornDate.SelectedDate.Value;

                if (int.TryParse(TxtGenderId.Text, out int gId))
                    _selectedPerson.Id_gender = new Gender { Id = gId };

                int result = await _apiService.UpdateAPerson(_selectedPerson);

                if (result > 0)
                {
                    MessageBox.Show("עודכן בהצלחה", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadPeople();
                    ClearFields();
                }
                else
                {
             
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה מפורטת מהשרת:\n{ex.Message}\n\n{ex.InnerException?.Message}", "שגיאה קריטית בעדכון", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPerson == null) return;

            var res = MessageBox.Show("האם למחוק את המשתמש?", "אישור מחיקה", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                if (await _apiService.DeleteAPerson(_selectedPerson.Id) > 0)
                {
                    MessageBox.Show("נמחק בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadPeople();
                    ClearFields();
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) => ClearFields();

        private void ClearFields()
        {
            _selectedPerson = null;
            TxtNumId.Clear(); TxtUserName.Clear(); TxtFirstName.Clear(); TxtLastName.Clear();
            TxtEmail.Clear(); TxtPhone.Clear(); TxtPassword.Clear(); TxtGenderId.Clear();
            DpBornDate.SelectedDate = null;
            PeopleDataGrid.SelectedItem = null;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.GoBack();
        }
    }
}