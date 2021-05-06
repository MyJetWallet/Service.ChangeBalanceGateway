using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.ChangeBalanceGateway.Settings
{
    public class SettingsModel
    {
        [YamlProperty("ChangeBalanceGateway.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("ChangeBalanceGateway.MatchingEngineApiGrpcServiceUrl")]
        public string MatchingEngineApiGrpcServiceUrl { get; set; }

        [YamlProperty("ChangeBalanceGateway.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("ChangeBalanceGateway.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }

        [YamlProperty("ChangeBalanceGateway.BalanceHistoryWriterGrpcServiceUrl")]
        public string BalanceHistoryWriterGrpcServiceUrl { get; set; }

        [YamlProperty("ChangeBalanceGateway.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("ChangeBalanceGateway.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }
}