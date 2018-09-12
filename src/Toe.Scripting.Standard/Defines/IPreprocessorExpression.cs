namespace Toe.Scripting.Defines
{
    public interface IPreprocessorExpression
    {
        FlatExpression AsFlatExpression();
        TreeExpression AsTreeExpression();
    }
}