using Avalonia.Controls;

namespace Lalamachine.Desktop.Views
{
    public partial class QueueControl : UserControl
    {
        public QueueControl()
        {
            InitializeComponent();
            TrackGrid.Attach(QueueGrid);
            TrackGrid.EnableReorder(QueueGrid);
        }
    }
}
