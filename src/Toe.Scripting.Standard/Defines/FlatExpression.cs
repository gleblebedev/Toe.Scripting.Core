using System;
using System.Collections.Generic;
using System.Linq;

namespace Toe.Scripting.Defines
{
    public class FlatExpression : PreprocessorExpression
    {
        private static readonly FlatExpressionLine[] emptyLines = new FlatExpressionLine[0];
        private readonly FlatExpressionLine[] _lines;
        private readonly Operands _operands;

        public FlatExpression()
        {
            _operands = Operands.Empty;
            _lines = emptyLines;
        }

        public FlatExpression(Operands operands, params FlatExpressionLine[] lines)
        {
            _operands = operands;
            _lines = lines;
        }

        public override Operands Operands => _operands;

        public IReadOnlyList<FlatExpressionLine> Lines => _lines;

        public override FlatExpression AsFlatExpression()
        {
            return this;
        }

        public override TreeExpression AsTreeExpression()
        {
            return new TreeExpression(this);
        }

        public FlatExpression And(FlatExpression b)
        {
            return And(this, b);
        }

        public static FlatExpression Not(FlatExpression a)
        {
            if (a._operands.Count >= 64)
                throw new NotImplementedException();
            var maxLines = 1ul << a._operands.Count;
            var expectedLines = (1ul << a._operands.Count)- (ulong)a._lines.Length;
            var lines = new FlatExpressionLine[expectedLines];
            long index = 0;
            long j = 0;
            ulong i = 0;
            for (; i < maxLines && index < a._lines.LongLength; ++i)
            {
                if (i < a._lines[index].Mask)
                {
                    lines[j] = new FlatExpressionLine(i);
                    ++j;
                }
                else
                {
                    ++index;
                }
            }
            for (; i < maxLines;++i)
            {
                lines[j] = new FlatExpressionLine(i);
                ++j;
            }
            return new FlatExpression(a.Operands, lines.ToArray());
        }

        public static FlatExpression And(FlatExpression a, FlatExpression b)
        {
            var allOperands = new Operands(a.Operands.Concat(b.Operands).Distinct().OrderBy(_ => _));
            var expandedA = a.ExpandTo(allOperands).Lines.ToList();
            expandedA.Sort();
            var expandedB = b.ExpandTo(allOperands).Lines.ToList();
            expandedB.Sort();

            var list = new List<FlatExpressionLine>();
            using (var iA = expandedA.GetEnumerator())
            {
                using (var iB = expandedB.GetEnumerator())
                {
                    var hasA = iA.MoveNext();
                    var hasB = iB.MoveNext();
                    while (hasA && hasB)
                        if (iA.Current.Mask == iB.Current.Mask)
                        {
                            list.Add(iA.Current);
                            hasA = iA.MoveNext();
                            hasB = iB.MoveNext();
                        }
                        else if (iA.Current.Mask < iB.Current.Mask)
                        {
                            hasA = iA.MoveNext();
                        }
                        else
                        {
                            hasB = iB.MoveNext();
                        }

                    return new FlatExpression(allOperands, list.ToArray());
                }
            }
        }

        public FlatExpression Or(FlatExpression b)
        {
            return Or(this, b);
        }

        public static FlatExpression Or(FlatExpression a, FlatExpression b)
        {
            var allOperands = new Operands(a.Operands.Concat(b.Operands).Distinct().OrderBy(_ => _));
            var expandedA = a.ExpandTo(allOperands).Lines.ToList();
            expandedA.Sort();
            var expandedB = b.ExpandTo(allOperands).Lines.ToList();
            expandedB.Sort();

            var list = new List<FlatExpressionLine>();
            using (var iA = expandedA.GetEnumerator())
            {
                using (var iB = expandedB.GetEnumerator())
                {
                    var hasA = iA.MoveNext();
                    var hasB = iB.MoveNext();
                    while (hasA && hasB)
                        if (iA.Current.Mask == iB.Current.Mask)
                        {
                            list.Add(iA.Current);
                            hasA = iA.MoveNext();
                            hasB = iB.MoveNext();
                        }
                        else if (iA.Current.Mask < iB.Current.Mask)
                        {
                            list.Add(iA.Current);
                            hasA = iA.MoveNext();
                        }
                        else
                        {
                            list.Add(iB.Current);
                            hasB = iB.MoveNext();
                        }

                    while (hasA)
                    {
                        list.Add(iA.Current);
                        hasA = iA.MoveNext();
                    }

                    while (hasB)
                    {
                        list.Add(iB.Current);
                        hasB = iB.MoveNext();
                    }

                    return new FlatExpression(allOperands, list.ToArray());
                }
            }
        }

        public FlatExpression ExpandTo(Operands operands)
        {
            var map = OperandsMap.Create(_operands, operands);

            return new FlatExpression(operands, map.Variator.GetVariations(_lines));
        }

        public override string ToString()
        {
            if (_lines.Length == 0)
                return False;
            return string.Join(" || ", _lines.Select(_ => _.ToString(_operands)));
        }
    }
}