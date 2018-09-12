using System;

namespace Toe.Scripting.Helpers
{
    public sealed class LinkHelper<T>
    {
        public LinkHelper(PinHelper<T> from, PinHelper<T> to)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));
            if (from.Type != to.Type)
                throw new InvalidOperationException("Pin type mismatch ("+ from.Type+", "+ to.Type+")");
            From = from;
            To = to;
        }

        public PinHelper<T> From { get; }
        public PinHelper<T> To { get; }

        public override string ToString()
        {
            return string.Format("({0}) -> ({1})", From, To);
        }
    }
}