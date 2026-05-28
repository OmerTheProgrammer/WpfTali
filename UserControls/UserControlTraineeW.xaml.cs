 using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Model;

namespace WpfTali.UserControls
{
    public partial class UserControlTraineeW : UserControl
    {
        public UserControlTraineeW()
        {
            InitializeComponent();
        }

        public UserControlTraineeW(List<WorkoutDisplayItem> myWorkouts)
        {
            InitializeComponent();

            if (myWorkouts != null)
            {
                MyWorkoutsListBox.ItemsSource = new ObservableCollection<WorkoutDisplayItem>(myWorkouts);
            }
        }
    }
}