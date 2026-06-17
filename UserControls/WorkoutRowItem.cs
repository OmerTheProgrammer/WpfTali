using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfTali.UserControls
{
    public  class WorkoutRowItem
    {
        public int RelationId { get; set; } //id של WORKOUTS_OF_TRAINERS
        public int KindId { get; set; }     
        public string WorkoutName { get; set; }
        public int MaxPeople { get; set; }

        public Visibility ReadVisibility { get; set; } = Visibility.Visible;
        public Visibility EditVisibility { get; set; } = Visibility.Collapsed;
    }
}
