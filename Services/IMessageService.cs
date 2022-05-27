namespace API.Services {
    public interface IMessageService: IDisposable {
        public void Send(string type, string subject, object? jsonSerializableData, Type? dataSerializableType);
        // Will eventually add Complete, Deadletter, Defer, and Receive
    }
}
