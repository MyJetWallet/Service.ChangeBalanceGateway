using System;
using System.Text.Json;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using Service.ChangeBalanceGateway.Client;
using Service.ChangeBalanceGateway.Grpc.Models;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();


            var factory = new ChangeBalanceGatewayClientFactory("http://localhost:80");
            var client = factory.GetChangeBalanceService();

            var resp = await  client.ChangeBalanceAsync(new ChangeBalanceGrpcRequest()
            {
                ClientId = "test",
                WalletId = "test--default",
                BrokerId = "jetwallet",

                AssetSymbol = "USD",
                Amount = 100,
                OperationType = ChangeBalanceType.CryptoDeposit,
                TransactionId = Guid.NewGuid().ToString(),
                Comment = "Test deposit"
            
            });
            Console.WriteLine(JsonSerializer.Serialize(resp));

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
