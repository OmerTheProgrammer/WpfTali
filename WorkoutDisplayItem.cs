using System;
using System.ComponentModel;
using System.Windows.Media;
using Model; 

namespace WpfTali
{
    public class WorkoutDisplayItem : INotifyPropertyChanged
    {
        public List_of_Exc_workouts RawWorkout { get; set; }
        public Kinds_of_workouts WorkoutKind { get; set; }
        public Person TrainerPerson { get; set; }

        public string WorkoutName => WorkoutKind?.Name_of_workout ?? "אימון כללי";
        public string TrainerName => TrainerPerson != null ? $"{TrainerPerson.First_name} {TrainerPerson.Last_name}" : "מאמן";
        public string TimeStr => RawWorkout?.Workout_date.ToString("HH:mm") ?? "00:00";
        public int MaxParticipants => WorkoutKind?.Max_amount_of_people ?? 10;

        private int _currentBooked;
        public int CurrentBooked
        {
            get => _currentBooked;
            set { _currentBooked = value; OnPropertyChanged(nameof(CurrentBooked)); OnPropertyChanged(nameof(StatusText)); OnPropertyChanged(nameof(IsFull)); }
        }

        private bool _isUserRegistered;
        public bool IsUserRegistered
        {
            get => _isUserRegistered;
            set { _isUserRegistered = value; OnPropertyChanged(nameof(IsUserRegistered)); OnPropertyChanged(nameof(BackgroundColor)); }
        }

        public bool IsFull => CurrentBooked >= MaxParticipants;
        public string StatusText => IsFull ? $"מלא, ברשימת המתנה" : $"{CurrentBooked}/{MaxParticipants} רשומים";

        public Brush BackgroundColor => IsUserRegistered ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF0F5")) : Brushes.White;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}