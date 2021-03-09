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

            var resp = await  client.PciDssDepositAsync(new PciDssDepositGrpcRequest()
            {
                ClientId = "test-1",
                WalletId = "SP-test-1",
                BrokerId = "jetwallet",

                AssetSymbol = "USD",
                Amount = 150,
                TransactionId = Guid.NewGuid().ToString(),
                Comment = "Test pci/dss deposit"
            
            });
            Console.WriteLine(JsonSerializer.Serialize(resp));


            resp = await client.ManualChangeBalanceAsync(new ManualChangeBalanceGrpcRequest()
            {
                ClientId = "test-1",
                WalletId = "SP-test-1",
                BrokerId = "jetwallet",

                AssetSymbol = "USD",
                Amount = -150,
                TransactionId = Guid.NewGuid().ToString(),
                Comment = "Test manual withdrawal",
                Officer = "alexey"
            });
            Console.WriteLine(JsonSerializer.Serialize(resp));

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
