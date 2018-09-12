using System;
using System.Text;

namespace Toe.Scripting.Defines
{
    public struct FlatExpressionLine : IComparable<FlatExpressionLine>
    {
        public FlatExpressionLine(ulong mask)
        {
            Mask = mask;
        }

        public ulong Mask { get; }

        public override string ToString()
        {
            return Convert.ToString((long) Mask, 2);
        }

        public string ToString(Operands operands)
        {
            var sb = new StringBuilder();
            ulong mask = 1;
            if (operands.Count > 1) sb.Append("(");

            for (var index = 0; index < operands.Count; index++)
            {
                var operand = operands[index];
                if (index != 0) sb.Append("&&");
                if (0 == (mask & Mask)) sb.Append("!");
                sb.Append("defined(");
                sb.Append(operand);
                sb.Append(")");
                mask = mask << 1;
            }

            if (operands.Count > 1) sb.Append(")");

            return sb.ToString();
        }

        public bool IsSet(int index)
        {
            return 0 != (Mask & (1ul << index));
        }

        public int CompareTo(FlatExpressionLine other)
        {
            return Mask.CompareTo(other.Mask);
        }
    }
}