using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Toe.Scripting.WPF.Model;

namespace Toe.Scripting.WPF.Converters
{
    [ValueConversion(typeof(ConnectionPoints), typeof(Geometry))]
    public class PointsToSelectionConverter : IValueConverter
    {
        public double BezierOffset { get; set; } = 100;
        public double Distance { get; set; } = 10;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var points = value as ConnectionPoints;
            if (points != null)
            {
                var start = points.From;
                var up = new Vector(0, Distance * 0.5);
                var down = new Vector(0, -Distance * 0.5);
                var a = points.From;
                var b = new Point(points.From.X + BezierOffset, points.From.Y);
                var c = new Point(points.To.X - BezierOffset, points.To.Y);
                var d = points.To;


                var segments = new List<PathSegment>();
                segments.Add(new BezierSegment(b + up, c + up, d + up, true));
                segments.Add(new LineSegment(d + down, true));
                segments.Add(new BezierSegment(c + down, b + down, a + down, true));
                segments.Add(new LineSegment(a + up, true));
                var figure = new PathFigure(start + up, segments, true);
                var geometry = new PathGeometry();
                geometry.Figures.Add(figure);
                return geometry;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}