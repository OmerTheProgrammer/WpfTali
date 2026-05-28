
using System;
using System.Windows.Controls;

namespace WpfTali
{
    public partial class UserControlinformation : UserControl
    {
        public event EventHandler MyEvent;
        public ContantDetailsInform ContactData { get; set; }

        public UserControlinformation()
        {
            InitializeComponent();
        }
        public UserControlinformation(ContantDetailsInform contactData)
        {
            InitializeComponent();
            UpdateData(contactData);
        }
        public void UpdateData(ContantDetailsInform contactData)
        {
            if (contactData == null) return;

            ContactData = contactData;
            this.firstName.Text = contactData.ContactfirstName;
            this.lastName.Text = contactData.ContactlastName;
            this.email.Text = contactData.Contactemail;
            this.telephone.Text = contactData.Contactphone;
            this.genderDisplay.Text = contactData.Contactgender;
            this.dateOfBirth.SelectedDate = contactData.Contactborn;
            this.passwordDisplay.Text = contactData.Contactpassword;
            this.idCardDisplay.Text = contactData.ContactidCard; 


        }
    }
}
