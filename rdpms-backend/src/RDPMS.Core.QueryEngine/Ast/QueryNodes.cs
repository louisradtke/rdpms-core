namespace RDPMS.Core.QueryEngine.Ast;

/// <summary>
/// Base type for all normalized query AST nodes.
/// </summary>
public abstract record QueryNode;

/// <summary>
/// Logical conjunction over child queries.
/// </summary>
public sealed record AndQueryNode(IReadOnlyList<QueryNode> Children) : QueryNode;

/// <summary>
/// Logical disjunction over child queries.
/// </summary>
public sealed record OrQueryNode(IReadOnlyList<QueryNode> Children) : QueryNode;

/// <summary>
/// Logical negation over a single child query.
/// </summary>
public sealed record NotQueryNode(QueryNode Child) : QueryNode;

/// <summary>
/// Query consisting of one or more field-path constraints.
/// </summary>
public sealed record FieldQueryNode(IReadOnlyList<FieldPredicateNode> Predicates) : QueryNode;

/// <summary>
/// A single field-path constraint pair.
/// </summary>
public sealed record FieldPredicateNode(string FieldPath, ConstraintNode Constraint);

/// <summary>
/// Base type for all field constraint AST nodes.
/// </summary>
public abstract record ConstraintNode;

/// <summary>
/// Equality constraint.
/// </summary>
public sealed record EqConstraintNode(ScalarValue Value) : ConstraintNode;

/// <summary>
/// Non-equality constraint.
/// </summary>
public sealed record NeConstraintNode(ScalarValue Value) : ConstraintNode;

/// <summary>
/// Inclusion constraint against a list of scalar values.
/// </summary>
public sealed record InConstraintNode(IReadOnlyList<ScalarValue> Values) : ConstraintNode;

/// <summary>
/// Path existence constraint.
/// </summary>
public sealed record ExistsConstraintNode(bool ShouldExist) : ConstraintNode;

/// <summary>
/// Greater-than scalar comparison.
/// </summary>
public sealed record GtConstraintNode(ScalarValue Value) : ConstraintNode;

/// <summary>
/// Greater-than-or-equal scalar comparison.
/// </summary>
public sealed record GteConstraintNode(ScalarValue Value) : ConstraintNode;

/// <summary>
/// Less-than scalar comparison.
/// </summary>
public sealed record LtConstraintNode(ScalarValue Value) : ConstraintNode;

/// <summary>
/// Less-than-or-equal scalar comparison.
/// </summary>
public sealed record LteConstraintNode(ScalarValue Value) : ConstraintNode;

/// <summary>
/// Regular expression constraint against string values.
/// </summary>
public sealed record RegexConstraintNode(string Pattern) : ConstraintNode;

/// <summary>
/// Exact array length constraint.
/// </summary>
public sealed record SizeConstraintNode(int Length) : ConstraintNode;

/// <summary>
/// Lower bound array length constraint.
/// </summary>
public sealed record GteSizeConstraintNode(int MinimumLength) : ConstraintNode;

/// <summary>
/// Upper bound array length constraint.
/// </summary>
public sealed record LteSizeConstraintNode(int MaximumLength) : ConstraintNode;

/// <summary>
/// At least one element in the target array must match the nested element query.
/// </summary>
public sealed record ElemMatchConstraintNode(QueryNode ElementQuery) : ConstraintNode;

/// <summary>
/// All elements in the target array must match the nested element query.
/// </summary>
public sealed record AllElemMatchConstraintNode(QueryNode ElementQuery) : ConstraintNode;

/// <summary>
/// All listed scalar values must be present in the target array or flattened path values.
/// </summary>
public sealed record ContainsAllConstraintNode(IReadOnlyList<ScalarValue> Values) : ConstraintNode;
