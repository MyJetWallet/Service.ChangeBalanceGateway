# Service.ChangeBalanceGateway

![Release Service](https://github.com/MyJetWallet/Service.ChangeBalanceGateway/workflows/Release%20Service/badge.svg)

![Release API client nuget](https://github.com/MyJetWallet/Service.ChangeBalanceGateway/workflows/Release%20API%20client%20nuget/badge.svg)

![CI test build](https://github.com/MyJetWallet/Service.ChangeBalanceGateway/workflows/CI%20test%20build/badge.svg)

*Client library:* ![Nuget version](https://img.shields.io/nuget/v/MyJetWallet.Service.ChangeBalanceGateway.Client?label=MyJetWallet.Service.ChangeBalanceGateway.Client&style=social)

# How to use

## Registration client

install client

`dotnet add package MyJetWallet.Service.ChangeBalanceGateway.Client`

### register client in Autofac

```csharp
public class Startup
{
	public void ConfigureContainer(ContainerBuilder builder)
	{
		builder.RegisterSpotChangeBalanceGatewayClient(Settings.changeBalanceGatewayGrpcServiceUrl)
	}
}
```

using after
```csharp
public class MyService
{
	private ISpotChangeBalanceService _service;

	public MyService(ISpotChangeBalanceService service)
	{
		_service = service;
	}

	public async Task MyCall()
	{
	    request = new ChangeBalanceGrpcRequest() {....};

		var resp = await _service.ChangeBalanceAsync()

		if (!resp.Result)
		{
			Console.WriteLine($"Cannot change balance. Error message: {resp.ErrorMessage}")
		}
	}
}
```

### create client manually

```csharp

  var factory = new ChangeBalanceGatewayClientFactory(changeBalanceGatewayGrpcServiceUrl);
  ISpotChangeBalanceService service = factory.GetChangeBalanceService();

```
