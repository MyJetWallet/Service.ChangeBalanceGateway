using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.MatchingEngine.Grpc;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using Service.AssetsDictionary.Client;
using Service.BalanceHistory.Client;
using Service.ChangeBalanceGateway.Settings;
using Service.ClientWallets.Client;
using SimpleTrading.SettingsReader;

namespace Service.ChangeBalanceGateway.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMatchingEngineGrpcClient(cashServiceGrpcUrl: Program.Settings.MatchingEngineCashServiceGrpcUrl);

            var myNoSqlClient = new MyNoSqlTcpClient(
                () => SettingsReader.ReadSettings<SettingsModel>(Program.SettingsFileName).MyNoSqlReaderHostPort,
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder.RegisterInstance(myNoSqlClient).AsSelf().SingleInstance();

            builder.RegisterAssetsDictionaryClients(myNoSqlClient);

            builder.RegisterClientWalletsClientsWithoutCache(Program.Settings.ClientWalletsGrpcServiceUrl);

            builder.RegisterBalanceHistoryOperationInfoClient(Program.Settings.BalanceHistoryWriterGrpcServiceUrl);
        }
    }
}