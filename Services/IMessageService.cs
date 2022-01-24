namespace API.Services {
    public interface IMessageService: IDisposable {
        public void Send(string message);
        // Will eventually add Complete, Deadletter, Defer, and Receive
    }
}
