using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public interface ICircuitBreakerService
{
    Task<T> ExecuteAsync<T>(string operationKey, Func<Task<T>> operation, Func<Task<T>> fallbackOperation);
    bool IsClosed(string operationKey);
}

public class CircuitBreakerService(
    ILogger<CircuitBreakerService> logger
) : ICircuitBreakerService
{
    private readonly Dictionary<string, CircuitBreaker> _circuitBreakers = [];
    private readonly Lock _lock = new();

    public async Task<T> ExecuteAsync<T>(string operationKey, Func<Task<T>> operation, Func<Task<T>> fallbackOperation)
    {
        var circuitBreaker = GetOrCreateCircuitBreaker(operationKey);

        if (circuitBreaker.State == CircuitState.Open)
        {
            logger.LogWarning("Circuit breaker is OPEN for operation {OperationKey}, using fallback", operationKey);
            return await fallbackOperation();
        }

        try
        {
            var result = await operation();
            circuitBreaker.OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Operation {OperationKey} failed, recording failure", operationKey);
            circuitBreaker.OnFailure();
            
            if (circuitBreaker.State == CircuitState.Open)
            {
                logger.LogWarning("Circuit breaker OPENED for operation {OperationKey}, using fallback", operationKey);
                return await fallbackOperation();
            }
            
            throw;
        }
    }

    public bool IsClosed(string operationKey)
    {
        var circuitBreaker = GetOrCreateCircuitBreaker(operationKey);
        return circuitBreaker.State == CircuitState.Closed;
    }

    private CircuitBreaker GetOrCreateCircuitBreaker(string operationKey)
    {
        lock (_lock)
        {
            if (!_circuitBreakers.ContainsKey(operationKey))
            {
                _circuitBreakers[operationKey] = new CircuitBreaker(
                    failureThreshold: 5,
                    timeout: TimeSpan.FromMinutes(1),
                    logger);
            }
            return _circuitBreakers[operationKey];
        }
    }
}

public class CircuitBreaker(
    int failureThreshold,
    TimeSpan timeout,
    ILogger logger
)
{
    private int _failureCount;
    private DateTime _lastFailureTime;
    private CircuitState _state = CircuitState.Closed;

    public CircuitState State => _state;

    public void OnSuccess()
    {
        if (_state == CircuitState.HalfOpen)
        {
            logger.LogInformation("Circuit breaker CLOSED after successful operation");
            _state = CircuitState.Closed;
            _failureCount = 0;
        }
    }

    public void OnFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;

        if (_state == CircuitState.Closed && _failureCount >= failureThreshold)
        {
            logger.LogWarning("Circuit breaker OPENED due to failure threshold reached");
            _state = CircuitState.Open;
        }
        else if (_state == CircuitState.HalfOpen)
        {
            logger.LogWarning("Circuit breaker OPENED after failure in half-open state");
            _state = CircuitState.Open;
        }
    }

    public bool CanAttemptReset()
    {
        return _state == CircuitState.Open && DateTime.UtcNow - _lastFailureTime >= timeout;
    }
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}
