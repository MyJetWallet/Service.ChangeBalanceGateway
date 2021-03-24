using System.Runtime.Serialization;

namespace Service.ChangeBalanceGateway.Grpc.Models
{
    [DataContract]
    public class BlockchainFeeApplyGrpcRequest
    {
        [DataMember(Order = 1)] public string TransactionId { get; set; }
        [DataMember(Order = 2)] public string WalletId { get; set; }
        [DataMember(Order = 3)] public string AssetSymbol { get; set; }
        [DataMember(Order = 3)] public double FeeAmount { get; set; }
    }
}