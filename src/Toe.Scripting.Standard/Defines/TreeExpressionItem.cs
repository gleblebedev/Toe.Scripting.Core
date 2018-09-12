using System.Text;

namespace Toe.Scripting.Defines
{
    public class TreeExpressionItem
    {
        public static readonly TreeExpressionItem Never = new TreeExpressionItem(TreeExpressionItemType.Never, 0);
        public static readonly TreeExpressionItem Always = new TreeExpressionItem(TreeExpressionItemType.Always, 0);

        private TreeExpressionItem(TreeExpressionItemType type, int index)
        {
            Type = type;
            Index = index;
        }

        public TreeExpressionItem(int index, TreeExpressionItem ifDefined, TreeExpressionItem ifNotDefined)
        {
            IfDefined = ifDefined;
            IfNotDefined = ifNotDefined;
            Type = TreeExpressionItemType.Or;
            Index = index;
        }

        public TreeExpressionItemType Type { get; }
        public int Index { get; }
        public TreeExpressionItem IfDefined { get; }
        public TreeExpressionItem IfNotDefined { get; }

        public bool? AsBool()
        {
            if (Type == TreeExpressionItemType.Always)
                return true;
            if (Type == TreeExpressionItemType.Never)
                return false;
            return null;
        }

        public string ToString(Operands operands)
        {
            if (Type == TreeExpressionItemType.Always)
                return PreprocessorExpression.True;
            if (Type == TreeExpressionItemType.Never)
                return PreprocessorExpression.False;

            var ifDef = IfDefined.AsBool();
            var ifNotDef = IfNotDefined.AsBool();

            var sb = new StringBuilder();
            if (ifDef != false)
            {
                if (ifNotDef != false) sb.Append("(");

                {
                    if (ifDef != true) sb.Append("(");

                    sb.Append("defined(");
                    sb.Append(operands[Index]);
                    sb.Append(")");
                    if (ifDef != true)
                    {
                        sb.Append(" && ");
                        sb.Append(IfDefined.ToString(operands));
                        sb.Append(")");
                    }
                }
                if (ifNotDef != false)
                {
                    sb.Append(" || ");
                    if (ifNotDef != true) sb.Append("(");

                    sb.Append("!defined(");
                    sb.Append(operands[Index]);
                    sb.Append(")");
                    if (ifNotDef != true)
                    {
                        sb.Append(" && ");
                        sb.Append(IfNotDefined.ToString(operands));
                        sb.Append(")");
                    }

                    sb.Append(")");
                }
            }
            else
            {
                if (ifNotDef != true) sb.Append("(");

                sb.Append("!defined(");
                sb.Append(operands[Index]);
                sb.Append(")");
                if (ifNotDef != true)
                {
                    sb.Append(" && ");
                    sb.Append(IfNotDefined.ToString(operands));
                    sb.Append(")");
                }
            }

            return sb.ToString();
        }
    }
}