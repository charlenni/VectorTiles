namespace VectorTiles.MapboxGL.Expressions
{
    public interface IExpression
    {
        object Evaluate(EvaluationContext ctx);

        object PossibleOutputs();
    }
}
