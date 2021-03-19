using System.Runtime.Serialization;

namespace Service.ChangeBalanceGateway.Grpc.Models
{
    [DataContract]
    public class ChangeBalanceGrpcResponse
    {
        [DataMember(Order = 1)] public bool Result { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public string TransactionId { get; set; }
        [DataMember(Order = 4)] public ErrorCodeEnum ErrorCode { get; set; }

        public enum ErrorCodeEnum
        {
            Ok,
            MeError,
            LowBalance,
            Duplicate,
            AssetDoNotFound,
            AssetIsDisabled,
            WalletDoNotFound,
            BadRequest
        }
    }
}