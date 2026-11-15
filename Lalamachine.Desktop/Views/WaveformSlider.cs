using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LibLala.Audio.Analysis;

namespace Lalamachine.Desktop.Views
{
    // A slider over the waveform of a track, its value the position in seconds: in bars around a line through the middle,
    // in the played brush up to the value. Just the line until there is a waveform. Its theme is in Styles.axaml, a track
    // as large as the slider that seeks to wherever it's clicked or dragged.
    internal sealed class WaveformSlider : Slider
    {
        public static readonly StyledProperty<Waveform?> WaveformProperty = AvaloniaProperty.Register<WaveformSlider, Waveform?>(nameof(Waveform));
        public static readonly StyledProperty<IBrush?> PlayedBrushProperty = AvaloniaProperty.Register<WaveformSlider, IBrush?>(nameof(PlayedBrush));
        public static readonly StyledProperty<IBrush?> UnplayedBrushProperty = AvaloniaProperty.Register<WaveformSlider, IBrush?>(nameof(UnplayedBrush));

        // Whole device-independent pixels, so that the bars are sharp: 2 wide, 1 apart.
        private const double BarWidth = 2;
        private const double BarStep = 3;
        // Silence still shows where the track is. Even heights, so that bars are centered on whole pixels too.
        private const double LineHeight = 2;

        // Drawn again only when what they show or the size changes, not as the value goes on.
        private StreamGeometry? _bars;
        private Size _barsSize;

        static WaveformSlider()
        {
            AffectsRender<WaveformSlider>(ValueProperty, MinimumProperty, MaximumProperty, WaveformProperty, PlayedBrushProperty, UnplayedBrushProperty);
        }

        public Waveform? Waveform
        {
            get => GetValue(WaveformProperty);
            set => SetValue(WaveformProperty, value);
        }

        public IBrush? PlayedBrush
        {
            get => GetValue(PlayedBrushProperty);
            set => SetValue(PlayedBrushProperty, value);
        }

        public IBrush? UnplayedBrush
        {
            get => GetValue(UnplayedBrushProperty);
            set => SetValue(UnplayedBrushProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var size = Bounds.Size;
            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            var bars = Bars(size);
            var range = Maximum - Minimum;
            var played = range > 0 ? Math.Clamp((Value - Minimum) / range, 0, 1) * size.Width : 0;
            using (context.PushClip(new Rect(0, 0, played, size.Height)))
            {
                context.DrawGeometry(PlayedBrush, null, bars);
            }
            using (context.PushClip(new Rect(played, 0, size.Width - played, size.Height)))
            {
                context.DrawGeometry(UnplayedBrush, null, bars);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == WaveformProperty || change.Property == MinimumProperty || change.Property == MaximumProperty)
            {
                _bars = null;
            }
        }

        private StreamGeometry Bars(Size size)
        {
            if (_bars is not null && _barsSize == size)
            {
                return _bars;
            }

            var middle = Math.Round(size.Height / 2);
            var geometry = new StreamGeometry();
            using (var figures = geometry.Open())
            {
                if (Waveform is null)
                {
                    AddRectangle(figures, new Rect(0, middle - (LineHeight / 2), size.Width, LineHeight));
                }
                else
                {
                    // As many as fit, the gap after the last one may be off the edge. Each as long as BarStep is on the
                    // slider, so that a bar shows what plays where it would seek to.
                    var count = (int)Math.Floor((size.Width - BarWidth) / BarStep) + 1;
                    var levels = Waveform.Bars(count, (Maximum - Minimum) * count * BarStep / size.Width);
                    for (var b = 0; b < count; b++)
                    {
                        var height = Math.Max(LineHeight, 2 * Math.Round(levels[b] * size.Height / 2));
                        AddRectangle(figures, new Rect(b * BarStep, middle - (height / 2), BarWidth, height));
                    }
                }
            }
            _bars = geometry;
            _barsSize = size;
            return geometry;
        }

        private static void AddRectangle(StreamGeometryContext figures, Rect rectangle)
        {
            figures.BeginFigure(rectangle.TopLeft, isFilled: true);
            figures.LineTo(rectangle.TopRight);
            figures.LineTo(rectangle.BottomRight);
            figures.LineTo(rectangle.BottomLeft);
            figures.EndFigure(isClosed: true);
        }
    }
}
