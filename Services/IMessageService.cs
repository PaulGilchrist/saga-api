namespace API.Services {
    public interface IMessageService: IDisposable {
        public void Send(string queueName, string type, object? jsonSerializableData, Type? dataSerializableType);
        // Will eventually add Complete, Deadletter, Defer, and Receive
    }
}
