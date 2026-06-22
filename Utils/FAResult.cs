namespace Shortenkai.Utils
{
    public class FAResult<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? ErrorMessage { get; }
        public bool IsNotFound { get; }

        private FAResult(T value) => (IsSuccess, Value) = (true, value);
        private FAResult(string error, bool isNotFound) => (IsSuccess, ErrorMessage, IsNotFound) = (false, error, isNotFound);

        public static FAResult<T> Success(T value) => new(value);
        public static FAResult<T> NotFound(string message) => new(message, true);
        public static FAResult<T> Failure(string message) => new(message, false);
    }

}
