using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    public class MassTransitMessage
    {
        public string MessageId { get; set; }
        public string CorrelationId { get; set; }
        public string ConversationId { get; set; }
        public string InitiatorId { get; set; }
        public string RequestId { get; set; }
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public string ResponseAddress { get; set; }
        public string FaultAddress { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public DateTime? SentTime { get; set; }
        public IDictionary<string, object> Headers { get; set; }
        public object Message { get; set; }
        public string[] MessageType { get; set; }
        public HostInfo Host { get; set; }
    }

    public class HostInfo
    {
        public string MachineName { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string Assembly { get; set; }
        public string AssemblyVersion { get; set; }
        public string FrameworkVersion { get; set; }
        public string MassTransitVersion { get; set; }
        public string OperatingSystemVersion { get; set; }
    }
}
