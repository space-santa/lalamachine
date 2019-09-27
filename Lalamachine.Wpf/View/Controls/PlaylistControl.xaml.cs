using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Lalamachine.Wpf.ViewModel;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for PlaylistControl.xaml
    /// </summary>
    public partial class PlaylistControl : UserControl
    {
        GridViewColumnHeader? _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

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

                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                    Sort(sortBy, direction);

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
                }
            }
        }

        private void Sort(string? sortBy, ListSortDirection direction)
        {
            if (sortBy is null) return;

            var dataView = CollectionViewSource.GetDefaultView(PlaylistView.ItemsSource);
            dataView.SortDescriptions.Clear();
            var sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
    }
}
