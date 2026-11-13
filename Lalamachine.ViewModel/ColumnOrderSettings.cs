using System;
using System.Collections.Generic;
using System.Linq;
using Settings;

namespace Lalamachine.ViewModel
{
    // The order the user dragged the columns of a grid of tracks to, by the names of the columns. One file for all grids:
    // there must be only one of these, a second one would save over what the first one saved.
    public class ColumnOrderSettings : AppSettingsBase
    {
        private const char Separator = ',';

        public ColumnOrderSettings() : this(new SettingsFile("Lalamachine.Desktop", "ColumnOrderSettings")) { }

        public ColumnOrderSettings(SettingsFile file) : base(file) { }

        // A column that isn't in the saved order, e.g. one that came with an update, goes at the end. One that isn't in
        // the grid any more is left out.
        public IReadOnlyList<string> OrderOf(string grid, IReadOnlyList<string> defaultOrder)
        {
            ArgumentNullException.ThrowIfNull(defaultOrder);

            var saved = GetString("", grid).Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            return saved.Where(defaultOrder.Contains).Concat(defaultOrder).Distinct().ToList();
        }

        // The default order isn't saved: it's the same as nothing saved, which follows the default when an update changes it.
        public void SetOrderOf(string grid, IReadOnlyList<string> order, IReadOnlyList<string> defaultOrder)
        {
            ArgumentNullException.ThrowIfNull(order);

            if (order.SequenceEqual(defaultOrder))
            {
                ResetOrderOf(grid);
                return;
            }

            Set(string.Join(Separator, order), grid);
        }

        // Nothing saved is the default order, also when an update changes it.
        public void ResetOrderOf(string grid)
        {
            Set("", grid);
        }
    }
}
