using Application.Abstractions.Messaging;
using Domain.Shared;
using MediatR;

namespace Application.Commands.ReadThrough;

public sealed record ReadThroughCommand(
    string Key,
    Func<Task<Result<byte[]>>> FallbackDataSource,
    TimeSpan AbsoluteExpiration,
    TimeSpan? SlidingExpiration = null
) : ICommand<byte[]>;