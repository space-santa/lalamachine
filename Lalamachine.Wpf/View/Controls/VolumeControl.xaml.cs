using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lalamachine.Wpf.View.Controls
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class MutedDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool muted = (bool)value;
            return muted ? "🔇" : "🔊";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public partial class VolumeControl : UserControl, INotifyPropertyChanged
    {
        private bool muted;
        public bool Muted
        {
            get => muted;
            set
            {
                muted = value;
                OnPropertyChanged("Muted");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public VolumeControl()
        {
            InitializeComponent();
            Muted = false;
        }


        private void MutedButton_Click(object sender, RoutedEventArgs e)
        {
            Muted = !Muted;
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
