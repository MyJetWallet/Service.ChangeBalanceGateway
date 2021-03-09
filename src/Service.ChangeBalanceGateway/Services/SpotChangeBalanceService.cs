﻿using System;
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
using Service.BalanceHistory.Domain.Models;
using Service.BalanceHistory.Grpc;
using Service.ChangeBalanceGateway.Grpc;
using Service.ChangeBalanceGateway.Grpc.Models;
using Service.ClientWallets.Grpc;
using SimpleTrading.Abstraction.Trading.BalanceOperations;

namespace Service.ChangeBalanceGateway.Services
{
    public class SpotChangeBalanceService: ISpotChangeBalanceService
    {
        private readonly ILogger<SpotChangeBalanceService> _logger;
        private readonly ICashServiceClient _cashServiceClient;
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;
        private readonly IClientWalletService _clientWalletService;
        private readonly IWalletBalanceUpdateOperationInfoService _balanceUpdateOperationInfoService;

        public SpotChangeBalanceService(ILogger<SpotChangeBalanceService> logger, 
            ICashServiceClient cashServiceClient,
            IAssetsDictionaryClient assetsDictionaryClient,
            IClientWalletService clientWalletService,
            IWalletBalanceUpdateOperationInfoService balanceUpdateOperationInfoService)
        {
            _logger = logger;
            _cashServiceClient = cashServiceClient;
            _assetsDictionaryClient = assetsDictionaryClient;
            _clientWalletService = clientWalletService;
            _balanceUpdateOperationInfoService = balanceUpdateOperationInfoService;
        }


        public async Task<ChangeBalanceGrpcResponse> PciDssDepositAsync(PciDssDepositGrpcRequest request)
        {
            _logger.LogInformation($"Change balance request: {JsonConvert.SerializeObject(request)}");

            if (request.Amount <= 0)
            {
                _logger.LogError("PciDss cannot decrease balance. Amount: {amount}, TransactionId: {transactionId}", request.Amount, request.TransactionId);
                return new ChangeBalanceGrpcResponse()
                {
                    Result = false,
                    TransactionId = request.TransactionId,
                };
            }

            var result =  await ChangeBalanceAsync(request.TransactionId, request.ClientId, request.WalletId, request.Amount, request.AssetSymbol,
                request.Comment, request.BrokerId, request.Agent, ChangeBalanceType.PciDssDeposit, "pcidss");

            if (!result.Result)
                _logger.LogError($"Cannot apply 'PciDssDeposit'. Message: {result.ErrorMessage}. Request: {JsonConvert.SerializeObject(request)}");

            return result;
        }

        public async Task<ChangeBalanceGrpcResponse> ManualChangeBalanceAsync(ManualChangeBalanceGrpcRequest request)
        {
            _logger.LogInformation($"Manual change balance request: {JsonConvert.SerializeObject(request)}");

            var type = request.Amount > 0 ? ChangeBalanceType.ManualDeposit : ChangeBalanceType.ManualWithdrawal;

            var result = await ChangeBalanceAsync(request.TransactionId, request.ClientId, request.WalletId, request.Amount, request.AssetSymbol,
                request.Comment, request.BrokerId, request.Agent, type, request.Officer);

            if (!result.Result)
                _logger.LogError($"Cannot apply 'PciDssDeposit'. Message: {result.ErrorMessage}. Request: {JsonConvert.SerializeObject(request)}");

            return result;
        }


        private async Task<ChangeBalanceGrpcResponse> ChangeBalanceAsync(
            string transactionId, string clientId, string walletId, double amount, string assetSymbol,
            string comment, string brokerId,  AgentInfo agent, ChangeBalanceType type, string changer)

        {
            var asset = _assetsDictionaryClient.GetAssetById(new AssetIdentity()
            {
                BrokerId = brokerId,
                Symbol = assetSymbol
            });

            //todo: убрать тут передачу бренда в принципе
            var wallets = await _clientWalletService.GetWalletsByClient(new JetClientIdentity(brokerId, "default-brand", clientId));
            var wallet = wallets?.Wallets.FirstOrDefault(e => e.WalletId == walletId);

            if (asset == null)
            {
                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = transactionId,
                    Result = false,
                    ErrorMessage = "Cannot change balance, asset do not found"
                };
            }

            if (!asset.IsEnabled)
            {
                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = transactionId,
                    Result = false,
                    ErrorMessage = "Cannot change balance, asset is Disabled."
                };
            }

            if (wallet == null)
            {
                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = transactionId,
                    Result = false,
                    ErrorMessage = "Cannot change balance, wallet do not found."
                };
            }

            await _balanceUpdateOperationInfoService.AddOperationInfoAsync(new WalletBalanceUpdateOperationInfo(
                transactionId,
                comment,
                type.ToString(),
                agent?.ApplicationName ?? "none",
                agent?.ApplicationEnvInfo ?? "none",
                changer));

            var meResp = await _cashServiceClient.CashInOutAsync(new CashInOutOperation()
            {
                Id = transactionId,
                MessageId = transactionId,

                BrokerId = brokerId,
                AccountId = clientId,
                WalletId = walletId,

                Fees = { }, //todo: calculate fee for deposit from fee service

                AssetId = asset.Symbol,
                Volume = amount.ToString(CultureInfo.InvariantCulture),
                Description = $"{comment} [{changer}]",
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            });

            if (meResp.Status != Status.Ok && meResp.Status != Status.Duplicate)
            {
                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = transactionId,
                    Result = false,
                    ErrorMessage = $"Cannot change balance, ME error: {meResp.Status}, reason: {meResp.StatusReason}"
                };
            }

            return new ChangeBalanceGrpcResponse()
            {
                TransactionId = transactionId,
                Result = true
            };
        }

        
    }
}
