using SimpleTrading.SettingsReader;

namespace Service.ChangeBalanceGateway.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("ChangeBalanceGateway.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("ChangeBalanceGateway.MatchingEngine.CashServiceGrpcUrl")]
        public string MatchingEngineCashServiceGrpcUrl { get; set; }

        [YamlProperty("ChangeBalanceGateway.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("ChangeBalanceGateway.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }
    }
}