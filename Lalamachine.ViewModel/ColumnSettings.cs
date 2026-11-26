using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Settings;

namespace Lalamachine.ViewModel
{
    // The order the user dragged the columns of a grid of tracks to, and their widths, by the names of the columns. One
    // file for all grids: there must be only one of these, a second one would save over what the first one saved.
    public class ColumnSettings : AppSettingsBase
    {
        private const char Separator = ',';
        private const char WidthSeparator = '=';
        private const string WidthsKey = ".Widths";
        // Wider than any screen, a width past it was edited into the file. The grid can't have one that isn't finite.
        private const double MaxWidth = 10000;

        // The file is still named after what it had at first, the order, so that the saved orders stay.
        public ColumnSettings() : this(new SettingsFile("Lalamachine.Desktop", "ColumnOrderSettings")) { }

        public ColumnSettings(SettingsFile file) : base(file) { }

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

        // The columns that the user made wider or narrower than they start, in whole pixels. A column that isn't in it
        // is as wide as it starts, also when an update changes that.
        public IReadOnlyDictionary<string, double> WidthsOf(string grid)
        {
            var widths = new Dictionary<string, double>();
            foreach (var entry in GetString("", grid + WidthsKey).Split(Separator, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = entry.Split(WidthSeparator);
                if (parts.Length == 2 && parts[0].Length > 0 && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var width) && width > 0 && width <= MaxWidth)
                {
                    widths[parts[0]] = width;
                }
            }
            return widths;
        }

        // Only the ones that aren't as wide as they start, for the same reason as the order.
        public void SetWidthsOf(string grid, IReadOnlyDictionary<string, double> widths, IReadOnlyDictionary<string, double> defaultWidths)
        {
            ArgumentNullException.ThrowIfNull(widths);
            ArgumentNullException.ThrowIfNull(defaultWidths);

            var changed = widths
                .Select(x => (Name: x.Key, Width: Math.Round(x.Value)))
                .Where(x => !defaultWidths.TryGetValue(x.Name, out var defaultWidth) || x.Width != Math.Round(defaultWidth))
                .Select(x => $"{x.Name}{WidthSeparator}{x.Width.ToString(CultureInfo.InvariantCulture)}");
            Set(string.Join(Separator, changed), grid + WidthsKey);
        }

        public void ResetWidthsOf(string grid)
        {
            Set("", grid + WidthsKey);
        }
    }
}
