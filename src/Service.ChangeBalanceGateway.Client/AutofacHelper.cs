using Autofac;
using Service.ChangeBalanceGateway.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.ChangeBalanceGateway.Client
{
    public static class AutofacHelper
    {
        public static void RegisterSpotChangeBalanceGatewayClient(this ContainerBuilder builder, string changeBalanceGatewayGrpcServiceUrl)
        {
            var factory = new ChangeBalanceGatewayClientFactory(changeBalanceGatewayGrpcServiceUrl);

            builder.RegisterInstance(factory.GetChangeBalanceService()).As<ISpotChangeBalanceService>().SingleInstance();
        }
    }
}