using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lalamachine.Wpf.ViewModel;
using LibLala.DomainPrimitives;

namespace Lalamachine.Wpf.View.Controls
{
    /// <summary>
    /// Interaction logic for LibraryPlaylistControl.xaml
    /// </summary>
    public partial class LibraryPlaylistControl : UserControl
    {
        private GridViewColumnHeader? _lastHeaderClicked;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public LibraryPlaylistControl()
        {
            InitializeComponent();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (PlaylistBaseViewModel)DataContext;
            viewModel.PlayTrackCommand.Execute(LibraryPlaylistView.SelectedItem);
        }


        private void LibraryPlaylistViewColumnHeader_Click(object sender, RoutedEventArgs e)
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

                    var message = new SortMessage(_lastDirection, new PlaylistColumn(_lastHeaderClicked.Content.ToString()));
                    var viewModel = (LibraryPlaylistViewModel)DataContext;
                    viewModel.SortCommand.Execute(message);
                }
            }
        }
    }
}
