﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using ME.Contracts.Api;
using ME.Contracts.Api.IncomingMessages;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Assets;
using MyJetWallet.Domain.Transactions;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Service.AssetsDictionary.Client;
using Service.BalanceHistory.Domain.Models;
using Service.BalanceHistory.Grpc;
using Service.ChangeBalanceGateway.Grpc;
using Service.ChangeBalanceGateway.Grpc.Models;
using Service.ClientWallets.Grpc;
using Service.Fees.Client;
using Service.Fees.Domain.Models;
using FeeType = ME.Contracts.Api.IncomingMessages.FeeType;
using Status = OpenTelemetry.Trace.Status;

namespace Service.ChangeBalanceGateway.Services
{
    public class SpotChangeBalanceService : ISpotChangeBalanceService
    {
        private readonly ILogger<SpotChangeBalanceService> _logger;
        private readonly CashService.CashServiceClient _cashServiceClient;
        private readonly IAssetsDictionaryClient _assetsDictionaryClient;
        private readonly IClientWalletService _clientWalletService;
        private readonly IWalletBalanceUpdateOperationInfoService _balanceUpdateOperationInfoService;
        private readonly IAssetFeesClient _assetFeesClient;

        public SpotChangeBalanceService(ILogger<SpotChangeBalanceService> logger,
            CashService.CashServiceClient cashServiceClient,
            IAssetsDictionaryClient assetsDictionaryClient,
            IClientWalletService clientWalletService,
            IWalletBalanceUpdateOperationInfoService balanceUpdateOperationInfoService,
            IAssetFeesClient assetFeesClient)
        {
            _logger = logger;
            _cashServiceClient = cashServiceClient;
            _assetsDictionaryClient = assetsDictionaryClient;
            _clientWalletService = clientWalletService;
            _balanceUpdateOperationInfoService = balanceUpdateOperationInfoService;
            _assetFeesClient = assetFeesClient;
        }


        public async Task<ChangeBalanceGrpcResponse> PciDssDepositAsync(PciDssDepositGrpcRequest request)
        {
            _logger.LogInformation($"Change balance request: {JsonConvert.SerializeObject(request)}");

            request.ClientId.AddToActivityAsTag("clientId");
            request.BrokerId.AddToActivityAsTag("brokerId");
            request.WalletId.AddToActivityAsTag("walletId");

            if (request.Amount <= 0)
            {
                _logger.LogError("PciDss cannot decrease balance. Amount: {amount}, TransactionId: {transactionId}",
                    request.Amount, request.TransactionId);

                Activity.Current?.SetStatus(Status.Error);
                Activity.Current?.AddTag("message", "PciDss cannot decrease balance");

                return new ChangeBalanceGrpcResponse()
                {
                    Result = false,
                    TransactionId = request.TransactionId,
                    ErrorMessage = "PciDss cannot decrease balance.",
                    ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.BadRequest
                };
            }

            var result = await ChangeBalanceAsync(request.TransactionId, request.ClientId, request.WalletId,
                request.Amount, request.AssetSymbol,
                request.Comment, request.BrokerId, request.Agent, ChangeBalanceType.PciDssDeposit, "pcidss",
                string.Empty, TransactionStatus.Confirmed, string.Empty);

            if (!result.Result)
            {
                _logger.LogError(
                    $"Cannot apply 'PciDssDeposit'. Message: {result.ErrorMessage}. Request: {JsonConvert.SerializeObject(request)}");
                Activity.Current?.SetStatus(Status.Error);
                Activity.Current?.AddTag("message", "Cannot apply 'PciDssDeposit'");
                result.AddToActivityAsJsonTag("change-balance-response");
            }

            return result;
        }

        public async Task<ChangeBalanceGrpcResponse> ManualChangeBalanceAsync(ManualChangeBalanceGrpcRequest request)
        {
            _logger.LogInformation($"Manual change balance request: {JsonConvert.SerializeObject(request)}");

            request.ClientId.AddToActivityAsTag("clientId");
            request.BrokerId.AddToActivityAsTag("brokerId");
            request.WalletId.AddToActivityAsTag("walletId");

            var type = request.Amount > 0 ? ChangeBalanceType.ManualDeposit : ChangeBalanceType.ManualWithdrawal;

            if (string.IsNullOrEmpty(request.Officer))
            {
                _logger.LogError(
                    "Manual change deposit cannot be executed without Officer name. TransactionId: {transactionId}",
                    request.TransactionId);
                Activity.Current?.SetStatus(Status.Error);
                Activity.Current?.AddTag("message", "Manual change deposit cannot be executed without Officer name");
                return new ChangeBalanceGrpcResponse()
                {
                    Result = false,
                    TransactionId = request.TransactionId,
                    ErrorMessage = "Manual change deposit cannot be executed without Officer name",
                    ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.BadRequest
                };
            }

            request.Officer.AddToActivityAsTag("officer");

            var result = await ChangeBalanceAsync(request.TransactionId, request.ClientId, request.WalletId,
                request.Amount, request.AssetSymbol,
                request.Comment, request.BrokerId, request.Agent, type, request.Officer, string.Empty,
                TransactionStatus.Confirmed, string.Empty, true);

            if (!result.Result)
            {
                _logger.LogError(
                    $"Cannot apply 'ManualChangeBalance'. Message: {result.ErrorMessage}. Request: {JsonConvert.SerializeObject(request)}");
                Activity.Current?.SetStatus(Status.Error);
                Activity.Current?.AddTag("message", "Cannot apply 'ManualChangeBalance'");
                result.AddToActivityAsJsonTag("change-balance-response");
            }

            return result;
        }

        public async Task<ChangeBalanceGrpcResponse> BlockchainDepositAsync(BlockchainDepositGrpcRequest request)
        {
            _logger.LogInformation($"Blockchain deposit request: {JsonConvert.SerializeObject(request)}");

            request.ClientId.AddToActivityAsTag("clientId");
            request.BrokerId.AddToActivityAsTag("brokerId");
            request.WalletId.AddToActivityAsTag("walletId");

            if (request.Amount <= 0)
            {
                _logger.LogError(
                    "Blockchain deposit cannot decrease balance. Amount: {amount}, TransactionId: {transactionId}",
                    request.Amount, request.TransactionId);
                Activity.Current?.SetStatus(Status.Error);
                Activity.Current?.AddTag("message", "Blockchain deposit cannot decrease balance");
                return new ChangeBalanceGrpcResponse()
                {
                    Result = false,
                    TransactionId = request.TransactionId,
                    ErrorMessage = "Blockchain deposit cannot decrease balance.",
                    ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.BadRequest
                };
            }

            var result = await ChangeBalanceAsync(request.TransactionId, request.ClientId, request.WalletId,
                request.Amount, request.AssetSymbol,
                request.Comment, request.BrokerId, request.Agent, ChangeBalanceType.CryptoDeposit, request.Integration,
                request.Txid, TransactionStatus.Confirmed, String.Empty);

            if (!result.Result)
            {
                _logger.LogError(
                    $"Cannot apply 'Blockchain deposit'. Message: {result.ErrorMessage}. Request: {JsonConvert.SerializeObject(request)}");
                Activity.Current?.SetStatus(Status.Error);
                Activity.Current?.AddTag("message", "Cannot apply 'Blockchain deposit'");
                result.AddToActivityAsJsonTag("change-balance-response");
            }

            return result;
        }

        public async Task<ChangeBalanceGrpcResponse> BlockchainWithdrawalAsync(BlockchainWithdrawalGrpcRequest request)
        {
            _logger.LogInformation($"Blockchain withdrawal request: {JsonConvert.SerializeObject(request)}");

            request.ClientId.AddToActivityAsTag("clientId");
            request.BrokerId.AddToActivityAsTag("brokerId");
            request.WalletId.AddToActivityAsTag("walletId");

            if (request.Amount >= 0)
            {
                _logger.LogError(
                    "Blockchain withdrawal cannot increase balance. Amount: {amount}, TransactionId: {transactionId}",
                    request.Amount, request.TransactionId);
                Activity.Current?.SetStatus(Status.Error);
                Activity.Current?.AddTag("message", "Blockchain withdrawal cannot increase balance");
                return new ChangeBalanceGrpcResponse()
                {
                    Result = false,
                    TransactionId = request.TransactionId,
                    ErrorMessage = "Blockchain withdrawal cannot increase balance.",
                    ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.BadRequest
                };
            }

            var result = await ChangeBalanceAsync(request.TransactionId, request.ClientId, request.WalletId,
                request.Amount, request.AssetSymbol,
                request.Comment, request.BrokerId, request.Agent, ChangeBalanceType.CryptoWithdrawal,
                request.Integration, request.Txid, request.Status, request.WithdrawalAddress);

            if (!result.Result)
            {
                _logger.LogError(
                    $"Cannot apply 'Blockchain withdrawal'. Message: {result.ErrorMessage}. Request: {JsonConvert.SerializeObject(request)}");
                Activity.Current?.SetStatus(Status.Error);
                Activity.Current?.AddTag("message", "Cannot apply 'Blockchain deposit'");
                result.AddToActivityAsJsonTag("change-balance-response");
            }

            return result;
        }

        public async Task<ChangeBalanceGrpcResponse> BlockchainFeeApplyAsync(BlockchainFeeApplyGrpcRequest request)
        {
            _logger.LogInformation("BlockchainFeeApplyGrpcRequest receive: {jsonText}",
                JsonConvert.SerializeObject(request));

            request.BrokerId.AddToActivityAsTag("brokerId");
            request.WalletId.AddToActivityAsTag("walletId");

            var fees = _assetFeesClient.GetAssetFees(request.BrokerId, request.AssetSymbol, OperationType.Withdrawal);

            if (fees == null)
            {
                return new ChangeBalanceGrpcResponse()
                {
                    ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.Ok,
                    TransactionId = $"fee:{request.TransactionId}",
                    Result = true
                };
            }

            var result = await ChangeBalanceAsync(Guid.NewGuid().ToString(), fees.AccountId, fees.WalletId,
                request.FeeAmount, request.AssetSymbol,
                "Deduct actual fee", fees.BrokerId, null, ChangeBalanceType.CryptoWithdrawal, "BitGo",
                request.TransactionId, TransactionStatus.Confirmed, "Fees");

            if (!result.Result)
            {
                _logger.LogError(
                    $"Cannot apply 'Blockchain fee'. Message: {result.ErrorMessage}. Request: {JsonConvert.SerializeObject(request)}");
                Activity.Current?.SetStatus(Status.Error);
                Activity.Current?.AddTag("message", "Cannot apply 'Blockchain fee'");
                result.AddToActivityAsJsonTag("change-balance-response");
            }

            return result;
        }


        private async Task<ChangeBalanceGrpcResponse> ChangeBalanceAsync(string transactionId, string clientId,
            string walletId, double amount, string assetSymbol,
            string comment, string brokerId, AgentInfo agent, ChangeBalanceType type, string changer, string txid,
            TransactionStatus status, string withdrawalAddress, bool noFee = false)

        {
            var asset = _assetsDictionaryClient.GetAssetById(new AssetIdentity()
            {
                BrokerId = brokerId,
                Symbol = assetSymbol
            });

            //todo: убрать тут передачу бренда в принципе
            var wallets =
                await _clientWalletService.GetWalletsByClient(
                    new JetClientIdentity(brokerId, "default-brand", clientId));
            var wallet = wallets?.Wallets.FirstOrDefault(e => e.WalletId == walletId);

            if (asset == null)
            {
                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = transactionId,
                    Result = false,
                    ErrorMessage = "Cannot change balance, asset do not found",
                    ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.AssetDoNotFound
                };
            }

            if (!asset.IsEnabled)
            {
                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = transactionId,
                    Result = false,
                    ErrorMessage = "Cannot change balance, asset is Disabled.",
                    ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.AssetIsDisabled
                };
            }

            if (wallet == null)
            {
                return new ChangeBalanceGrpcResponse()
                {
                    TransactionId = transactionId,
                    Result = false,
                    ErrorMessage = "Cannot change balance, wallet do not found.",
                    ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.WalletDoNotFound
                };
            }

            await _balanceUpdateOperationInfoService.AddOperationInfoAsync(new WalletBalanceUpdateOperationInfo(
                transactionId,
                comment,
                type.ToString(),
                agent?.ApplicationName ?? "none",
                agent?.ApplicationEnvInfo ?? "none",
                changer,
                txid,
                status,
                withdrawalAddress,
                string.Empty
            ));

            var meRequest = new CashInOutOperation()
            {
                Id = transactionId,
                MessageId = transactionId,

                BrokerId = brokerId,
                AccountId = clientId,
                WalletId = walletId,

                AssetId = asset.Symbol,
                Volume = amount.ToString(CultureInfo.InvariantCulture),
                Description = $"{comment} [{changer}]",
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            if (noFee)
            {
                meRequest.Fees.Add(new Fee {Type = FeeType.NoFee});
            }

            var meResp = await _cashServiceClient.CashInOutAsync(meRequest);

            _logger.LogInformation("Me call. Request: {requestText}. Response: {responseText}",
                JsonConvert.SerializeObject(meRequest), JsonConvert.SerializeObject(meResp));

            if (meResp.Status != ME.Contracts.Api.IncomingMessages.Status.Ok &&
                meResp.Status != ME.Contracts.Api.IncomingMessages.Status.Duplicate)
            {
                var res = new ChangeBalanceGrpcResponse()
                {
                    TransactionId = transactionId,
                    Result = false,
                    ErrorMessage = $"Cannot change balance, ME error: {meResp.Status}, reason: {meResp.StatusReason}",
                    ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.MeError
                };

                if (meResp.Status == ME.Contracts.Api.IncomingMessages.Status.LowBalance)
                    res.ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.LowBalance;

                if (meResp.Status == ME.Contracts.Api.IncomingMessages.Status.Duplicate)
                    res.ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.Duplicate;
            }

            return new ChangeBalanceGrpcResponse()
            {
                TransactionId = transactionId,
                Result = true,
                ErrorCode = ChangeBalanceGrpcResponse.ErrorCodeEnum.Ok
            };
        }
    }
}