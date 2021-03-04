using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using JetBrains.Annotations;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.ChangeBalanceGateway.Grpc;

namespace Service.ChangeBalanceGateway.Client
{
    [UsedImplicitly]
    public class ChangeBalanceGatewayClientFactory
    {
        private readonly CallInvoker _channel;

        public ChangeBalanceGatewayClientFactory(string assetsDictionaryGrpcServiceUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(assetsDictionaryGrpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public IChangeBalanceService GetChangeBalanceService() => _channel.CreateGrpcService<IChangeBalanceService>();
    }
}
