using System.Collections.Generic;

namespace Toe.Scripting.Defines
{
    public class MatchingOperandsMap : OperandsMap
    {
        private AbstractVariator _variator;
        private readonly int _varitationBits;
        private readonly int _varitationOffset;

        public MatchingOperandsMap(Operands from, Operands to)
        {
            _varitationBits = to.Count - from.Count;
            _varitationOffset = from.Count;
        }

        public override int this[int index] => index;

        public override AbstractVariator Variator => _variator ?? (_variator = new MatchingVariator(this));

        internal class MatchingVariator : AbstractVariator
        {
            private readonly MatchingOperandsMap _map;

            public MatchingVariator(MatchingOperandsMap map)
            {
                _map = map;
            }

            public override FlatExpressionLine[] GetVariations(IReadOnlyList<FlatExpressionLine> lines)
            {
                var values = new FlatExpressionLine[lines.Count * (1 << _map._varitationBits)];
                var step = 1ul << _map._varitationOffset;
                var end = step << _map._varitationBits;
                var index = 0;
                foreach (var flatExpressionLine in lines)
                    for (ulong i = 0; i < end; i += step)
                    {
                        values[index] = new FlatExpressionLine(flatExpressionLine.Mask | i);
                        ++index;
                    }

                return values;
            }
        }
    }
}