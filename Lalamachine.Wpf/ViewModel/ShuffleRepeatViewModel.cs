using System;
using System.Windows.Input;

namespace Lalamachine.Wpf.ViewModel
{
    public enum ShuffleRepeatState { None, RepeatOne, RepeatAll, Shuffle };

    public class ChangeShuffleRepeatEventArgs : EventArgs
    {
        public ChangeShuffleRepeatEventArgs(ShuffleRepeatState shuffleRepeatState)
        {
            ShuffleRepeatState = shuffleRepeatState;
        }
        public ShuffleRepeatState ShuffleRepeatState { get; }
    }

    public class ShuffleRepeatViewModel : BaseNotifyPropertyChanged
    {
        public event EventHandler<ChangeShuffleRepeatEventArgs>? ShuffleRepeatChanged;
        protected virtual void InvokeShuffleRepeatChanged()
        {
            ShuffleRepeatChanged?.Invoke(this, new ChangeShuffleRepeatEventArgs(State));
        }

        private ShuffleRepeatState _state;
        public ShuffleRepeatState State
        {
            get => _state;
            set
            {
                _state = value;
                NotifyPropertyChanged();
                InvokeShuffleRepeatChanged();
            }
        }


        private readonly DelegateCommand _changeShuffleRepeatCommand;
        public ICommand ChangeShuffleRepeatCommand => _changeShuffleRepeatCommand;

        public ShuffleRepeatViewModel()
        {
            _changeShuffleRepeatCommand = new DelegateCommand(OnChangeShuffleRepeat);
        }

        private void OnChangeShuffleRepeat(object? commandParameter)
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
