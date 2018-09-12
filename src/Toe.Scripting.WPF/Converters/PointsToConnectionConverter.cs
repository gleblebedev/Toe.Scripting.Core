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
    public class PointsToConnectionConverter : IValueConverter
    {
        public double BezierOffset { get; set; } = 100;
        public double ArrowLength { get; set; } = 10;
        public double ArrowWidth { get; set; } = 4;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var points = value as ConnectionPoints;
            if (points != null)
            {
                var start = points.From;
                var a = points.From;
                var b = new Point(points.From.X + BezierOffset, points.From.Y);
                var c = new Point(points.To.X - BezierOffset, points.To.Y);
                var d = points.To;

                var arrowA = new Point(points.To.X - ArrowLength, points.To.Y - ArrowWidth);
                var arrowB = new Point(points.To.X - ArrowLength, points.To.Y + ArrowWidth);


                var segments = new List<PathSegment>();
                segments.Add(new BezierSegment(b, c, d, true));
                segments.Add(new LineSegment(arrowA, true));
                segments.Add(new LineSegment(arrowB, true));
                segments.Add(new LineSegment(d, true));
                var figure = new PathFigure(start, segments, false);
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