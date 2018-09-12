using System.Collections.Generic;
using System.Linq;

namespace Toe.Scripting.Defines
{
    public class UnmatchingOperandsMap : OperandsMap
    {
        private readonly int[] _indices;
        private readonly int _numTargetOperands;
        private AbstractVariator _variator;

        public UnmatchingOperandsMap(Operands from, Operands to)
        {
            _numTargetOperands = to.Count;
            UnusedMask = (1ul << to.Count) - 1;
            UnusedCount = to.Count - from.Count;
            var lookup = to.Select((_, i) => new KeyValuePair<string, int>(_, i))
                .ToDictionary(_ => _.Key, _ => _.Value);
            _indices = new int[from.Count];
            for (var index = 0; index < from.Count; index++)
            {
                var i = _indices[index] = lookup[from[index]];
                UnusedMask &= ~(1ul << i);
            }
        }

        public ulong UnusedMask { get; }

        public int UnusedCount { get; }

        public override int this[int index] => _indices[index];

        public override AbstractVariator Variator => _variator ?? (_variator = new UnmatchingVariator(this));

        public ulong Remap(ulong mask)
        {
            ulong res = 0;
            for (var i = 0; mask > 0 && i < _indices.Length; i++)
            {
                if (0 != (mask & 1)) res |= 1ul << _indices[i];

                mask >>= 1;
            }

            return res;
        }

        internal class UnmatchingVariator : AbstractVariator
        {
            private readonly UnmatchingOperandsMap _map;
            private readonly List<ulong> _variations;

            public UnmatchingVariator(UnmatchingOperandsMap map)
            {
                _map = map;
                var variationParts = new List<ulong>(map.UnusedCount);
                _variations = new List<ulong>(1 << map.UnusedCount);
                for (var index = 0; index < _map._numTargetOperands; ++index)
                {
                    var mask = 1ul << index;
                    if (0 != (map.UnusedMask & mask)) variationParts.Add(mask);
                }

                for (var i = 0ul; i < 1ul << map.UnusedCount; ++i)
                {
                    ulong mask = 0;
                    var partMask = i;
                    for (var j = 0; partMask != 0 && j < map.UnusedCount; ++j)
                    {
                        if (0 != (partMask & 1))
                            mask |= variationParts[j];
                        partMask >>= 1;
                    }

                    _variations.Add(mask);
                }
            }

            public override FlatExpressionLine[] GetVariations(IReadOnlyList<FlatExpressionLine> lines)
            {
                var newValues = new FlatExpressionLine[lines.Count * (1 << _map.UnusedCount)];

                var index = 0;
                foreach (var flatExpressionLine in lines)
                {
                    var mask = _map.Remap(flatExpressionLine.Mask);
                    foreach (var variation in _variations)
                    {
                        newValues[index] = new FlatExpressionLine(mask | variation);
                        ++index;
                    }
                }

                return newValues;
            }
        }
    }
}