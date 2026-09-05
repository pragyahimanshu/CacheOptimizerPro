using Application.Abstractions.Messaging;
using Domain.Shared;

namespace Application.Commands.WriteThrough;

public sealed record WriteThroughCommand(
    string Key,
    byte[] Value,
    TimeSpan AbsoluteExpiration,
    Func<Task<Result>> UpdateDataSource,
    TimeSpan? SlidingExpiration = null
) : ICommand;