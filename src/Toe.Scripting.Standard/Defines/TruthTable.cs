using System.Text;

namespace Toe.Scripting.Defines
{
    public class TruthTable
    {
        private readonly Operands _operands;
        private readonly uint[] _table;

        public TruthTable(FlatExpression expression)
        {
            _operands = expression.Operands;
            ulong size = 1;
            if (_operands.Count > 5)
                size = 1ul << (_operands.Count - 5);
            _table = new uint[size];
            foreach (var line in expression.Lines) this[line.Mask] = true;
        }

        public bool this[ulong mask]
        {
            get
            {
                var index = mask >> 5;
                return 0 != (_table[index] & (1 << (int) (mask & 31)));
            }
            set
            {
                var index = mask >> 5;
                var m = 1u << (int) (mask & 31);
                if (value)
                    _table[index] |= m;
                else
                    _table[index] &= ~m;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (_operands.Count <= 5)
            {
                for (ulong i = 0; i < 1ul << _operands.Count; ++i)
                    if (this[i])
                    {
                        if (sb.Length != 0)
                            sb.Append(" || ");
                        sb.Append(new FlatExpressionLine(i).ToString(_operands));
                    }
            }
            else
            {
                for (long i = 0; i < _table.LongLength; ++i)
                {
                    var m = _table[i];
                    for (var j = 0; m != 0 && j < 32; ++j)
                    {
                        if (0 != (m & 1))
                        {
                            if (sb.Length != 0)
                                sb.Append(" || ");
                            sb.Append(new FlatExpressionLine(((ulong) i << 5) | (ulong) j).ToString(_operands));
                        }

                        m >>= 1;
                    }
                }
            }

            return sb.ToString();
        }
    }
}