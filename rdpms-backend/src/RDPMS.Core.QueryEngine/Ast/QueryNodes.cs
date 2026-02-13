namespace RDPMS.Core.QueryEngine.Ast;

public abstract record QueryNode;

public sealed record AndQueryNode(IReadOnlyList<QueryNode> Children) : QueryNode;

public sealed record OrQueryNode(IReadOnlyList<QueryNode> Children) : QueryNode;

public sealed record NotQueryNode(QueryNode Child) : QueryNode;

public sealed record FieldQueryNode(IReadOnlyList<FieldPredicateNode> Predicates) : QueryNode;

public sealed record FieldPredicateNode(string FieldPath, ConstraintNode Constraint);

public abstract record ConstraintNode;

public sealed record EqConstraintNode(ScalarValue Value) : ConstraintNode;

public sealed record NeConstraintNode(ScalarValue Value) : ConstraintNode;

public sealed record InConstraintNode(IReadOnlyList<ScalarValue> Values) : ConstraintNode;

public sealed record ExistsConstraintNode(bool ShouldExist) : ConstraintNode;

public sealed record GtConstraintNode(ScalarValue Value) : ConstraintNode;

public sealed record GteConstraintNode(ScalarValue Value) : ConstraintNode;

public sealed record LtConstraintNode(ScalarValue Value) : ConstraintNode;

public sealed record LteConstraintNode(ScalarValue Value) : ConstraintNode;

public sealed record RegexConstraintNode(string Pattern) : ConstraintNode;

public sealed record SizeConstraintNode(int Length) : ConstraintNode;

public sealed record GteSizeConstraintNode(int MinimumLength) : ConstraintNode;

public sealed record LteSizeConstraintNode(int MaximumLength) : ConstraintNode;

public sealed record ElemMatchConstraintNode(QueryNode ElementQuery) : ConstraintNode;

public sealed record AllElemMatchConstraintNode(QueryNode ElementQuery) : ConstraintNode;

public sealed record ContainsAllConstraintNode(IReadOnlyList<ScalarValue> Values) : ConstraintNode;
