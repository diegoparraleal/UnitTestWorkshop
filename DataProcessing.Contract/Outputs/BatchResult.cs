using System.Collections.Immutable;
using DataProcessing.Contract.Domain;

namespace DataProcessing.Contract.Outputs;

public sealed record BatchResult(Invoice Invoice, bool Success, ImmutableList<string> Messages)
{
    public BatchResult AddFailure(string message) => new(Invoice, false, Messages.Add(message));
    public BatchResult AddFailures(IEnumerable<string> messages) => new(Invoice, false, Messages.AddRange(messages));

    public static bool IsValid(BatchResult batchResult) => batchResult.Success;
}