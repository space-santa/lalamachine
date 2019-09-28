using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lalamachine.Wpf.ViewModel;
using LibLala.DomainPrimitives;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for PlaylistControl.xaml
    /// </summary>
    public partial class PlaylistControl : UserControl
    {
        private GridViewColumnHeader? _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public PlaylistControl()
        {
            InitializeComponent();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (PlaylistViewModel)DataContext;
            viewModel.PlayTrackCommand.Execute(PlaylistView.SelectedItem);
        }

        // TODO: Add sort of the actual list to the ViewModel as a command. This does only sort the view which is useless.
        private void PlaylistViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.SetCurrentValue(GridViewColumn.HeaderTemplateProperty, Resources["HeaderTemplateArrowUp"] as DataTemplate);
                    }
                    else
                    {
                        headerClicked.Column.SetCurrentValue(GridViewColumn.HeaderTemplateProperty, Resources["HeaderTemplateArrowDown"] as DataTemplate);
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.SetCurrentValue(GridViewColumn.HeaderTemplateProperty, null);
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;

                    // TODO: Create an object that contains the relevant information for sorting and pass it to the command.
                    var message = new SortMessage(_lastDirection, new PlaylistColumn(_lastHeaderClicked.Content.ToString()));
                    var viewModel = (PlaylistViewModel)DataContext;
                    viewModel.SortCommand.Execute(message);
                }
            }
        }
    }
}
