namespace API.Services {
    public interface IMessageService: IDisposable {

        // Send message now or save the message to send later (delaySend)
        public void Send(string queueName, string type, object? jsonSerializableData, Type? dataSerializableType, bool delaySend = false);

        // Clear all delayed event messages
        public void ClearDelayed();

        // Send all delayed messages in FIFO order
        public void SendDelayed();

    }
}
