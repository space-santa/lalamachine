using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Lalamachine.Wpf.View.Controls
{
    public enum ShuffleRepeatState { None, RepeatOne, RepeatAll, Shuffle };

    public partial class ShuffleRepeatControl : UserControl, INotifyPropertyChanged
    {
        private ShuffleRepeatState state;
        public ShuffleRepeatState State
        {
            get => state;
            set
            {
                state = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ShuffleRepeatControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case ShuffleRepeatState.RepeatOne:
                    State = ShuffleRepeatState.RepeatAll;
                    break;
                case ShuffleRepeatState.RepeatAll:
                    State = ShuffleRepeatState.Shuffle;
                    break;
                case ShuffleRepeatState.None:
                    State = ShuffleRepeatState.RepeatOne;
                    break;
                default:
                    State = ShuffleRepeatState.None;
                    break;
            }
        }
    }
}
