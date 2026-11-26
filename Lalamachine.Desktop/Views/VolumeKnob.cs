using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace Lalamachine.Desktop.Views
{
    // The volume from 0 to 100, as a knob: dragged up or right it turns up, down or left it turns down, with Control held in
    // small steps. The wheel and the arrow keys turn it too. A click in its middle mutes and unmutes. IsAdjusting while it's
    // turned, and for a moment after a turn by the wheel or a key, to show the volume then. Its colours are in Styles.axaml.
    internal sealed class VolumeKnob : Control
    {
        public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<VolumeKnob, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay, coerce: (_, value) => Math.Clamp(value, 0, 100));
        public static readonly StyledProperty<bool> IsMutedProperty = AvaloniaProperty.Register<VolumeKnob, bool>(nameof(IsMuted), defaultBindingMode: BindingMode.TwoWay);
        public static readonly DirectProperty<VolumeKnob, bool> IsAdjustingProperty = AvaloniaProperty.RegisterDirect<VolumeKnob, bool>(nameof(IsAdjusting), x => x.IsAdjusting);
        public static readonly StyledProperty<IBrush?> ArcBrushProperty = AvaloniaProperty.Register<VolumeKnob, IBrush?>(nameof(ArcBrush));
        public static readonly StyledProperty<IBrush?> TrackBrushProperty = AvaloniaProperty.Register<VolumeKnob, IBrush?>(nameof(TrackBrush));
        public static readonly StyledProperty<IBrush?> MutedBrushProperty = AvaloniaProperty.Register<VolumeKnob, IBrush?>(nameof(MutedBrush));
        public static readonly StyledProperty<IBrush?> BodyBrushProperty = AvaloniaProperty.Register<VolumeKnob, IBrush?>(nameof(BodyBrush));
        public static readonly StyledProperty<IBrush?> BodyBorderBrushProperty = AvaloniaProperty.Register<VolumeKnob, IBrush?>(nameof(BodyBorderBrush));
        public static readonly StyledProperty<IBrush?> PointerBrushProperty = AvaloniaProperty.Register<VolumeKnob, IBrush?>(nameof(PointerBrush));

        // From the bottom left round over the top to the bottom right, clockwise as the screen's y goes down.
        private const double StartAngle = 135;
        private const double SweepAngle = 270;
        private const double ArcThickness = 6;
        // Of the radius inside the arc.
        private const double BodyRatio = 0.8;
        // Of the body's radius, what a click mutes in.
        private const double MiddleRatio = 0.55;
        // Per device-independent pixel the pointer moves, and per notch of the wheel or press of a key.
        private const double DragStep = 0.5;
        private const double FineDragStep = 0.1;
        private const double Step = 5;
        private const double FineStep = 1;
        // A click wobbles a little, a turn moves further.
        private const double DragThreshold = 3;

        // Over the middle a click, elsewhere a turn.
        private static readonly Cursor ClickCursor = new(StandardCursorType.Hand);
        private static readonly Cursor TurnCursor = new(StandardCursorType.SizeNorthSouth);

        private readonly DispatcherTimer _hideTimer = new() { Interval = TimeSpan.FromSeconds(1) };
        private bool _isAdjusting;
        private bool _isPressed;
        private bool _isDragging;
        private bool _isPressedInMiddle;
        private Point _pressedAt;
        private Point _lastPoint;

        static VolumeKnob()
        {
            AffectsRender<VolumeKnob>(ValueProperty, IsMutedProperty, ArcBrushProperty, TrackBrushProperty, MutedBrushProperty, BodyBrushProperty, BodyBorderBrushProperty, PointerBrushProperty);
            FocusableProperty.OverrideDefaultValue<VolumeKnob>(true);
        }

        public VolumeKnob()
        {
            _hideTimer.Tick += (sender, e) =>
            {
                _hideTimer.Stop();
                if (!_isDragging)
                {
                    IsAdjusting = false;
                }
            };
        }

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool IsMuted
        {
            get => GetValue(IsMutedProperty);
            set => SetValue(IsMutedProperty, value);
        }

        public bool IsAdjusting
        {
            get => _isAdjusting;
            private set => SetAndRaise(IsAdjustingProperty, ref _isAdjusting, value);
        }

        public IBrush? ArcBrush
        {
            get => GetValue(ArcBrushProperty);
            set => SetValue(ArcBrushProperty, value);
        }

        public IBrush? TrackBrush
        {
            get => GetValue(TrackBrushProperty);
            set => SetValue(TrackBrushProperty, value);
        }

        public IBrush? MutedBrush
        {
            get => GetValue(MutedBrushProperty);
            set => SetValue(MutedBrushProperty, value);
        }

        public IBrush? BodyBrush
        {
            get => GetValue(BodyBrushProperty);
            set => SetValue(BodyBrushProperty, value);
        }

        public IBrush? BodyBorderBrush
        {
            get => GetValue(BodyBorderBrushProperty);
            set => SetValue(BodyBorderBrushProperty, value);
        }

        public IBrush? PointerBrush
        {
            get => GetValue(PointerBrushProperty);
            set => SetValue(PointerBrushProperty, value);
        }

        private Point Center => new(Bounds.Width / 2, Bounds.Height / 2);
        private double ArcRadius => Math.Max(0, (Math.Min(Bounds.Width, Bounds.Height) / 2) - (ArcThickness / 2) - 1);
        private double BodyRadius => Math.Max(0, ArcRadius - (ArcThickness / 2)) * BodyRatio;

        private bool IsInMiddle(Point point)
        {
            var offset = point - Center;
            return Math.Sqrt((offset.X * offset.X) + (offset.Y * offset.Y)) <= BodyRadius * MiddleRatio;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (ArcRadius <= 0)
            {
                return;
            }

            var center = Center;
            var arcPen = new Pen(TrackBrush, ArcThickness, lineCap: PenLineCap.Round);
            context.DrawGeometry(null, arcPen, Arc(center, ArcRadius, StartAngle, SweepAngle));
            var sweep = SweepAngle * Value / 100;
            if (sweep > 0)
            {
                var valuePen = new Pen(IsMuted ? MutedBrush : ArcBrush, ArcThickness, lineCap: PenLineCap.Round);
                context.DrawGeometry(null, valuePen, Arc(center, ArcRadius, StartAngle, sweep));
            }

            var bodyRadius = BodyRadius;
            context.DrawEllipse(BodyBrush, new Pen(BodyBorderBrush, 1), center, bodyRadius, bodyRadius);

            // A line from near the edge of the body towards the middle, where the knob is turned to.
            var angle = (StartAngle + sweep) * Math.PI / 180;
            var direction = new Vector(Math.Cos(angle), Math.Sin(angle));
            var pointerPen = new Pen(PointerBrush, 3, lineCap: PenLineCap.Round);
            context.DrawLine(pointerPen, center + (direction * bodyRadius * 0.85), center + (direction * bodyRadius * 0.62));

            // What a click in the middle does.
            var glyph = new FormattedText(IsMuted ? "🔇" : "🔊", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, bodyRadius * 0.6, PointerBrush);
            context.DrawText(glyph, center - new Vector(glyph.Width / 2, glyph.Height / 2));
        }

        // Clockwise from the start angle, in degrees.
        private static StreamGeometry Arc(Point center, double radius, double startAngle, double sweepAngle)
        {
            static Point At(Point center, double radius, double degrees)
            {
                var radians = degrees * Math.PI / 180;
                return new Point(center.X + (radius * Math.Cos(radians)), center.Y + (radius * Math.Sin(radians)));
            }

            var geometry = new StreamGeometry();
            using (var figure = geometry.Open())
            {
                figure.BeginFigure(At(center, radius, startAngle), isFilled: false);
                figure.ArcTo(At(center, radius, startAngle + sweepAngle), new Size(radius, radius), 0, sweepAngle > 180, SweepDirection.Clockwise);
                figure.EndFigure(isClosed: false);
            }
            return geometry;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                return;
            }

            _isPressed = true;
            _isDragging = false;
            _pressedAt = e.GetPosition(this);
            _lastPoint = _pressedAt;
            _isPressedInMiddle = IsInMiddle(_pressedAt);
            e.Pointer.Capture(this);
            Focus();
            e.Handled = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (e is null)
            {
                return;
            }

            var point = e.GetPosition(this);
            Cursor = _isPressed || !IsInMiddle(point) ? TurnCursor : ClickCursor;
            if (!_isPressed)
            {
                return;
            }

            var fromPress = point - _pressedAt;
            if (!_isDragging && Math.Abs(fromPress.X) < DragThreshold && Math.Abs(fromPress.Y) < DragThreshold)
            {
                return;
            }

            _isDragging = true;
            IsAdjusting = true;
            // From where it was the last time, so that pressing or letting go of Control doesn't make it jump.
            var moved = point - _lastPoint;
            var step = e.KeyModifiers.HasFlag(KeyModifiers.Control) ? FineDragStep : DragStep;
            Value += (moved.X - moved.Y) * step;
            _lastPoint = point;
            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (e is null || !_isPressed)
            {
                return;
            }

            if (!_isDragging && _isPressedInMiddle && IsInMiddle(e.GetPosition(this)))
            {
                IsMuted = !IsMuted;
            }
            EndPress();
            e.Pointer.Capture(null);
            e.Handled = true;
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            EndPress();
        }

        private void EndPress()
        {
            _isPressed = false;
            if (_isDragging)
            {
                _isDragging = false;
                ShowForAMoment();
            }
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            Cursor = null;
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            if (e is null)
            {
                return;
            }

            // A notch of a mouse wheel is 1. A trackpad sends many small deltas, and 0 for a sideways swipe.
            Value += e.Delta.Y * (e.KeyModifiers.HasFlag(KeyModifiers.Control) ? FineStep : Step);
            ShowForAMoment();
            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e is null || e.Handled)
            {
                return;
            }

            var step = e.KeyModifiers.HasFlag(KeyModifiers.Control) ? FineStep : Step;
            switch (e.Key)
            {
                case Key.Up or Key.Right:
                    Value += step;
                    break;
                case Key.Down or Key.Left:
                    Value -= step;
                    break;
                case Key.Enter:
                    IsMuted = !IsMuted;
                    e.Handled = true;
                    return;
                default:
                    return;
            }
            ShowForAMoment();
            e.Handled = true;
        }

        private void ShowForAMoment()
        {
            IsAdjusting = true;
            _hideTimer.Stop();
            _hideTimer.Start();
        }
    }
}
