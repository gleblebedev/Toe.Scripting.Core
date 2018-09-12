using System.Collections.Generic;
using System.Linq;

namespace Toe.Scripting.Defines
{
    public abstract class OperandsMap
    {
        public abstract int this[int index] { get; }

        public abstract AbstractVariator Variator { get; }

        public static OperandsMap Create(Operands from, Operands to)
        {
            if (to.Count >= from.Count)
                if (from.Zip(to, (a, b) => a == b).All(_ => _))
                    return new MatchingOperandsMap(from, to);
            return new UnmatchingOperandsMap(from, to);
        }

        public abstract class AbstractVariator
        {
            public abstract FlatExpressionLine[] GetVariations(IReadOnlyList<FlatExpressionLine> lines);
        }
    }
}