using System;
using Avalonia.Controls.Primitives;

namespace Lalamachine.Desktop.Views
{
    // A toggle button that shows what it's bound to and doesn't change it itself: a click runs its command, and it's
    // checked when what the command did says so. Play doesn't always play, e.g. a track that can't be read, and the
    // button mustn't look like it does then. Looks like any other toggle button.
    internal sealed class BoundToggleButton : ToggleButton
    {
        protected override Type StyleKeyOverride => typeof(ToggleButton);

        protected override void Toggle()
        {
        }
    }
}
