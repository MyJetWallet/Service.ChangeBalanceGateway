namespace Service.ChangeBalanceGateway.Grpc.Models
{
    public enum ChangeBalanceType
    {
        Unknown = 0,
        
        PciDssDeposit = 1,
        CryptoDeposit = 2,
        ManualDeposit = 3,


        ManualWithdrawal = 11
    }
}