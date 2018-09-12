using System.Windows;

namespace Toe.Scripting.WPF.Model
{
    public class ConnectionPoints
    {
        public ConnectionPoints(Point from, Point to)
        {
            From = from;
            To = to;
        }

        public Point From { get; }

        public Point To { get; }
    }
}