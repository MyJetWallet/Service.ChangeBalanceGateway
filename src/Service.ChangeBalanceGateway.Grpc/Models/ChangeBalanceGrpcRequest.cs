using System.Runtime.Serialization;

namespace Service.ChangeBalanceGateway.Grpc.Models
{
    [DataContract]
    public class ChangeBalanceGrpcRequest
    {
        [DataMember(Order = 1)] public string TransactionId { get; set; }

        [DataMember(Order = 2)] public string ClientId { get; set; }

        [DataMember(Order = 3)] public string WalletId { get; set; }

        [DataMember(Order = 4)] public double Amount { get; set; }

        [DataMember(Order = 5)] public string AssetSymbol { get; set; }

        [DataMember(Order = 6)] public string Comment { get; set; }

        [DataMember(Order = 7)] public ChangeBalanceType OperationType { get; set; }

        [DataMember(Order = 8)] public string BrokerId { get; set; }

        [DataMember(Order = 9)] public AgentInfo Agent { get; set; } = new AgentInfo();
    }
}