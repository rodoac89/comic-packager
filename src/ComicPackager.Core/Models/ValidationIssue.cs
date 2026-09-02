namespace ComicPackager.Core.Models;

public sealed record ValidationIssue(string Code, string Message);

public sealed class ValidationResult
{
    public IReadOnlyList<ValidationIssue> Issues { get; }

    public ValidationResult(IReadOnlyList<ValidationIssue> issues) => Issues = issues;

    public bool IsValid => Issues.Count == 0;

    public static ValidationResult Ok() => new([]);

    public static ValidationResult Fail(params ValidationIssue[] issues) => new(issues);

    public string CombinedMessage => string.Join(Environment.NewLine, Issues.Select(i => i.Message));
}
