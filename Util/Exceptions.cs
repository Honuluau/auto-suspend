public class InsufficientStorageException : Exception {
    public InsufficientStorageException(string message): base(message) {}
}

public class FailedInternetConnectionException : Exception {
    public FailedInternetConnectionException(string message): base(message) {}
}