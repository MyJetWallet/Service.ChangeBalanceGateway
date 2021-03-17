using System.Runtime.Serialization;
using MyJetWallet.Domain.Transactions;

namespace Service.ChangeBalanceGateway.Grpc.Models
{
    [DataContract]
    public class BlockchainTransferGrpcRequest
    {
        public BlockchainTransferGrpcRequest()
        {
        }

        public BlockchainTransferGrpcRequest(string transactionId, string clientId, string walletId, double amount, string assetSymbol, string comment, string brokerId, string integration, string txid)
        {
            TransactionId = transactionId;
            ClientId = clientId;
            WalletId = walletId;
            Amount = amount;
            AssetSymbol = assetSymbol;
            Comment = comment;
            BrokerId = brokerId;
            Integration = integration;
            Txid = txid;
        }

        [DataMember(Order = 1)] public string TransactionId { get; set; }

        [DataMember(Order = 2)] public string ClientId { get; set; }

        [DataMember(Order = 3)] public string WalletId { get; set; }

        [DataMember(Order = 4)] public double Amount { get; set; }

        [DataMember(Order = 5)] public string AssetSymbol { get; set; }

        [DataMember(Order = 6)] public string Comment { get; set; }

        [DataMember(Order = 8)] public string BrokerId { get; set; }

        [DataMember(Order = 9)] public string Integration { get; set; }

        [DataMember(Order = 10)] public string Txid { get; set; }

        [DataMember(Order = 11)] public TransactionStatus Status { get; set; }

        [DataMember(Order = 12)] public AgentInfo Agent { get; set; } = new AgentInfo();
    }
}