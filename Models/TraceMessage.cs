using System;
using Newtonsoft.Json;

namespace API.Models {
    public class TraceMessage {
        public TraceMessage(string? Action, string? Name, string? Key, object? Value) {
            this.Action = Action;
            this.Name = Name;
            this.Key = Key;
            this.Value = Value;
        }
        public string? Action { get; set; }
        public string? Name { get; set; }
        public string? Key { get; set; }
        public object? Value { get; set; }
    }
}

