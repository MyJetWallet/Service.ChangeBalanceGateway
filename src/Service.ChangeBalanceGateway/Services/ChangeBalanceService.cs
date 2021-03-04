using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using ME.Contracts.Api.IncomingMessages;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Assets;
using MyJetWallet.MatchingEngine.Grpc.Api;
using Newtonsoft.Json;
using Service.AssetsDictionary.Client;
using Service.ChangeBalanceGateway.Grpc;
using Service.ChangeBalanceGateway.Grpc.Models;
using Service.ClientWallets.Grpc;

namespace Service.ChangeBalanceGateway.Services
{
    public class ChangeBalanceService: IChangeBalanceService
    {
        private readonly ILogger<ChangeBalanceService> _logger;
        private readonly ICashServiceClient _cashServiceClient;
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;
        private readonly IClientWalletService _clientWalletService;

        public ChangeBalanceService(ILogger<ChangeBalanceService> logger, 
            ICashServiceClient cashServiceClient,
            IAssetsDictionaryClient assetsDictionaryClient,
            IClientWalletService clientWalletService)
        {
            _logger = logger;
            _cashServiceClient = cashServiceClient;
            _assetsDictionaryClient = assetsDictionaryClient;
            _clientWalletService = clientWalletService;
        }


        public async Task<ChangeBalanceGrpcResponse> ChangeBalanceAsync(ChangeBalanceGrpcRequest request)
        {
            _logger.LogInformation($"Change balance request: {JsonConvert.SerializeObject(request)}");

            var asset = _assetsDictionaryClient.GetAssetById(new AssetIdentity()
            {
                BrokerId = request.BrokerId,
                Symbol = request.AssetSymbol
            });

            //todo: убрать тут передачу бренда в принципе
            var wallets = await _clientWalletService.GetWalletsByClient(new JetClientIdentity(request.BrokerId, "default-brand", request.ClientId));
            var wallet = wallets?.Wallets.FirstOrDefault(e => e.WalletId == request.WalletId);

            if (asset == null)
            {
                _logger.LogError($"Cannot change balance, asset do not found.  Request: {JsonConvert.SerializeObject(request)}");
                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = request.TransactionId,
                    Result = false,
                    ErrorMessage = "Cannot change balance, asset do not found"
                };
            }

            if (!asset.IsEnabled)
            {
                _logger.LogError($"Cannot change balance, asset is Disabled.  Request: {JsonConvert.SerializeObject(request)}");

                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = request.TransactionId,
                    Result = false,
                    ErrorMessage = "Cannot change balance, asset is Disabled."
                };
            }

            if (wallet == null)
            {
                _logger.LogError($"Cannot change balance, wallet do not found.  Request: {JsonConvert.SerializeObject(request)}");
                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = request.TransactionId,
                    Result = false,
                    ErrorMessage = "Cannot change balance, wallet do not found."
                };
            }

            var meResp = await _cashServiceClient.CashInOutAsync(new CashInOutOperation()
            {
                Id = request.TransactionId,
                MessageId = request.TransactionId,
                
                BrokerId = request.BrokerId,
                AccountId = request.ClientId,
                WalletId = request.WalletId,
                
                Fees = { }, //todo: calculate fee for deposit from fee service

                AssetId = asset.Symbol,
                Volume = request.Amount.ToString(CultureInfo.InvariantCulture),
                Description = request.Comment,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            });

            if (meResp.Status != Status.Ok)
            {
                _logger.LogError($"Cannot change balance, ME error: {meResp.Status}, reason: {meResp.StatusReason}.  Request: {JsonConvert.SerializeObject(request)}");

                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = request.TransactionId,
                    Result = false,
                    ErrorMessage = $"Cannot change balance, ME error: {meResp.Status}, reason: {meResp.StatusReason}"
                };
            }

            return new ChangeBalanceGrpcResponse()
            {
                TransactionId = request.TransactionId,
                Result = true
            };
        }
    }
}
