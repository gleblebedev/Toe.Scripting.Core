namespace Toe.Scripting.Defines
{
    public abstract class PreprocessorExpression : IPreprocessorExpression
    {
        public const string True = "true";

        public const string False = "false";
        public abstract Operands Operands { get; }
        public abstract FlatExpression AsFlatExpression();

        public abstract TreeExpression AsTreeExpression();

        public static IPreprocessorExpression Defined(string key)
        {
            return new FlatExpression(new Operands(key), new FlatExpressionLine(1));
        }

        public static IPreprocessorExpression NotDefined(string key)
        {
            return new FlatExpression(new Operands(key), new FlatExpressionLine(0));
        }
    }
}