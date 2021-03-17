using System.ServiceModel;
using System.Threading.Tasks;
using Service.ChangeBalanceGateway.Grpc.Models;

namespace Service.ChangeBalanceGateway.Grpc
{
    [ServiceContract]
    public interface ISpotChangeBalanceService
    {
        [OperationContract] 
        Task<ChangeBalanceGrpcResponse> PciDssDepositAsync(PciDssDepositGrpcRequest request);

        [OperationContract]
        Task<ChangeBalanceGrpcResponse> ManualChangeBalanceAsync(ManualChangeBalanceGrpcRequest request);

        [OperationContract]
        Task<ChangeBalanceGrpcResponse> BlockchainDepositAsync(BlockchainDepositGrpcRequest request);

        [OperationContract]
        Task<ChangeBalanceGrpcResponse> BlockchainWithdrawalAsync(BlockchainWithdrawalGrpcRequest request);
    }
}