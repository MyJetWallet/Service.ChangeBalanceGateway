using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Service.ChangeBalanceGateway.Grpc.Models
{
    [DataContract]
    public class AgentInfo
    {
        public static string AppName { get; set; }
        public static string AppEnvInfo { get; set; }

        public AgentInfo()
        {
            if (string.IsNullOrEmpty(AppName))
            {
                AppName = Environment.GetEnvironmentVariable("APP_VERSION") ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "none";
                AppEnvInfo = Environment.GetEnvironmentVariable("ENV_INFO");
            }

            ApplicationName = AppName;
            ApplicationEnvInfo = AppEnvInfo;
        }

        [DataMember(Order = 1)] public string ApplicationName { get; set; }
        [DataMember(Order = 2)] public string ApplicationEnvInfo { get; set; }
    }
}