using System.ServiceModel;
using System.Threading.Tasks;
using Service.ChangeBalanceGateway.Grpc.Models;

namespace Service.ChangeBalanceGateway.Grpc
{
    [ServiceContract]
    public interface ISpotChangeBalanceService
    {
        [OperationContract]
        Task<ChangeBalanceGrpcResponse> ChangeBalanceAsync(ChangeBalanceGrpcRequest request);
    }
}