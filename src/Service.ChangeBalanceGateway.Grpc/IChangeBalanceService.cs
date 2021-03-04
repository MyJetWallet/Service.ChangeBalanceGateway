using System.ServiceModel;
using System.Threading.Tasks;
using Service.ChangeBalanceGateway.Grpc.Models;

namespace Service.ChangeBalanceGateway.Grpc
{
    [ServiceContract]
    public interface IChangeBalanceService
    {
        [OperationContract]
        Task<ChangeBalanceGrpcResponse> ChangeBalanceAsync(ChangeBalanceGrpcRequest request);
    }
}