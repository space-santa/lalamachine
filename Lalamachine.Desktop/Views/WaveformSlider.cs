using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LibLala.Audio.Analysis;

namespace Lalamachine.Desktop.Views
{
    // A slider over the waveform of a track, its value the position in seconds: in bars around a line through the middle,
    // in the played brush up to the value. Until there is a waveform, and where there is none, a line with a knob at the
    // value. Its theme is in Styles.axaml, a track as large as the slider that seeks to wherever it's clicked or dragged.
    // Left to right only, like the waveform.
    internal sealed class WaveformSlider : Slider
    {
        public static readonly StyledProperty<Waveform?> WaveformProperty = AvaloniaProperty.Register<WaveformSlider, Waveform?>(nameof(Waveform));
        public static readonly StyledProperty<IBrush?> PlayedBrushProperty = AvaloniaProperty.Register<WaveformSlider, IBrush?>(nameof(PlayedBrush));
        public static readonly StyledProperty<IBrush?> UnplayedBrushProperty = AvaloniaProperty.Register<WaveformSlider, IBrush?>(nameof(UnplayedBrush));

        // In device-independent pixels, each made whole pixels on the screen so that the bars are sharp at any scaling: 2
        // wide, 1 apart.
        private const double BarWidth = 2;
        private const double BarGap = 1;
        // Silence still shows where the track is. Even heights, so that bars are centered on whole pixels too.
        private const double LineHeight = 2;
        private const double KnobRadius = 6;

        // Drawn again only when what they show, the size or the scaling changes, not as the value goes on.
        private StreamGeometry? _bars;
        private Size _barsSize;
        private double _barsScaling;

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

            var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
            var bars = Bars(size, scaling);
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
            if (Waveform is null)
            {
                // All of it inside the slider, at either end too.
                var x = Math.Clamp(played, KnobRadius, Math.Max(KnobRadius, size.Width - KnobRadius));
                context.DrawEllipse(PlayedBrush, null, new Point(x, Middle(size, scaling)), KnobRadius, KnobRadius);
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

        // On a whole pixel of the screen.
        private static double Middle(Size size, double scaling) => Math.Round(size.Height * scaling / 2) / scaling;

        private StreamGeometry Bars(Size size, double scaling)
        {
            if (_bars is not null && _barsSize == size && _barsScaling == scaling)
            {
                return _bars;
            }

            // In pixels of the screen from here on, and back in device-independent ones to draw.
            var width = size.Width * scaling;
            var middle = Middle(size, scaling) * scaling;
            var lineHeight = Even(LineHeight * scaling);
            var geometry = new StreamGeometry();
            using (var figures = geometry.Open())
            {
                if (Waveform is null)
                {
                    AddRectangle(figures, new Rect(0, middle - (lineHeight / 2), width, lineHeight), scaling);
                }
                else
                {
                    var barWidth = Math.Max(1, Math.Round(BarWidth * scaling, MidpointRounding.AwayFromZero));
                    var barStep = barWidth + Math.Max(1, Math.Round(BarGap * scaling, MidpointRounding.AwayFromZero));
                    // As many as fit, the gap after the last one may be off the edge. Each as long as its step is on the
                    // slider, so that a bar shows what plays where it would seek to.
                    var count = Math.Max(0, (int)Math.Floor((width - barWidth) / barStep) + 1);
                    var levels = Waveform.Bars(count, (Maximum - Minimum) * count * barStep / width);
                    for (var b = 0; b < count; b++)
                    {
                        var height = Math.Max(lineHeight, Even(levels[b] * size.Height * scaling));
                        AddRectangle(figures, new Rect(b * barStep, middle - (height / 2), barWidth, height), scaling);
                    }
                }
            }
            _bars = geometry;
            _barsSize = size;
            _barsScaling = scaling;
            return geometry;
        }

        private static double Even(double pixels) => 2 * Math.Round(pixels / 2, MidpointRounding.AwayFromZero);

        private static void AddRectangle(StreamGeometryContext figures, Rect pixels, double scaling)
        {
            var rectangle = pixels * (1 / scaling);
            figures.BeginFigure(rectangle.TopLeft, isFilled: true);
            figures.LineTo(rectangle.TopRight);
            figures.LineTo(rectangle.BottomRight);
            figures.LineTo(rectangle.BottomLeft);
            figures.EndFigure(isClosed: true);
        }
    }
}
