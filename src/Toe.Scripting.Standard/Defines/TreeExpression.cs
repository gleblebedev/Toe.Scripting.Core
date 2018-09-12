using System.Collections.Generic;
using System.Linq;

namespace Toe.Scripting.Defines
{
    public class TreeExpression : PreprocessorExpression, IPreprocessorExpression
    {
        private readonly Operands _operands;
        private readonly TreeExpressionItem _root;

        public TreeExpression()
        {
            _operands = Operands.Empty;
            _root = TreeExpressionItem.Never;
        }

        public TreeExpression(FlatExpression expression)
        {
            _operands = expression.Operands;
            var lines = expression.Lines.ToList();
            _root = Split(lines, _operands.Count - 1, 0, lines.Count);
        }

        public override Operands Operands => _operands;

        public override FlatExpression AsFlatExpression()
        {
            var queue = new Queue<TreeCurstor>();
            var listExpressions = new List<FlatExpressionLine>(4);
            queue.Enqueue(new TreeCurstor {Item = _root, Depth = 0, Mask = 0ul});
            while (queue.Count > 0)
            {
                var cursor = queue.Dequeue();
                if (cursor.Item.Type == TreeExpressionItemType.Never) continue;
                if (cursor.Item.Type == TreeExpressionItemType.Always)
                {
                    var maxValue = 1ul << (_operands.Count - cursor.Depth) << cursor.Depth;
                    for (ulong i = 0; i < maxValue; i += (ulong) cursor.Depth)
                        listExpressions.Add(new FlatExpressionLine(cursor.Mask | i));
                    continue;
                }

                queue.Enqueue(new TreeCurstor
                {
                    Item = cursor.Item.IfDefined,
                    Depth = cursor.Depth + 1,
                    Mask = cursor.Mask | (1ul << cursor.Depth)
                });
                queue.Enqueue(new TreeCurstor
                {
                    Item = cursor.Item.IfNotDefined,
                    Depth = cursor.Depth + 1,
                    Mask = cursor.Mask
                });
            }

            return new FlatExpression(_operands, listExpressions.ToArray());
        }

        public override TreeExpression AsTreeExpression()
        {
            return this;
        }

        private TreeExpressionItem Split(List<FlatExpressionLine> lines, int operandIndex, int start, int end)
        {
            if (start == end)
                return TreeExpressionItem.Never;
            var firstSet = start;
            while (firstSet < end)
            {
                if (lines[firstSet].IsSet(operandIndex))
                    break;
                ++firstSet;
            }

            for (var index = end - 1; index >= firstSet; --index)
                if (!lines[index].IsSet(operandIndex))
                {
                    var a = lines[index];
                    lines[index] = lines[firstSet];
                    lines[firstSet] = a;
                    ++firstSet;
                }

            if (operandIndex == 0)
            {
                // if both A and !A exist then it doesn't matter - it's always true;
                if (end - firstSet == 1 && firstSet - start == 1) return TreeExpressionItem.Always;
                return new TreeExpressionItem(operandIndex,
                    firstSet != end ? TreeExpressionItem.Always : TreeExpressionItem.Never,
                    start != firstSet ? TreeExpressionItem.Always : TreeExpressionItem.Never
                );
            }

            var ifDefOperand = Split(lines, operandIndex - 1, firstSet, end);
            var ifNotDefOperand = Split(lines, operandIndex - 1, start, firstSet);
            var ifDefBool = ifDefOperand.AsBool();
            var ifNotDefBool = ifNotDefOperand.AsBool();
            if (ifDefBool == true && ifNotDefBool == true)
                return TreeExpressionItem.Always;
            if (ifDefBool == false && ifNotDefBool == false)
                return TreeExpressionItem.Never;
            return new TreeExpressionItem(operandIndex, ifDefOperand, ifNotDefOperand);
        }

        public override string ToString()
        {
            return _root.ToString(_operands);
        }

        internal struct TreeCurstor
        {
            public TreeExpressionItem Item;
            public int Depth;
            public ulong Mask;
        }
    }
}