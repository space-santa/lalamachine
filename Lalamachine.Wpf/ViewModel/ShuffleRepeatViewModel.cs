using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace Lalamachine.Wpf.ViewModel
{
    public enum ShuffleRepeatState { None, RepeatOne, RepeatAll, Shuffle };

    class ShuffleRepeatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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


        private readonly DelegateCommand _changeShuffleRepeatCommand;
        public ICommand ChangeShuffleRepeatCommand => _changeShuffleRepeatCommand;

        public ShuffleRepeatViewModel()
        {
            _changeShuffleRepeatCommand = new DelegateCommand(OnChangeShuffleRepeat, (object commandParameter) => { return true; });
        }

        private void OnChangeShuffleRepeat(object commandParameter)
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
