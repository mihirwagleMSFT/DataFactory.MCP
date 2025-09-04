using System.Text.Json.Serialization;

namespace DataFactory.MCP.Models;

/// <summary>
/// Base gateway class
/// </summary>
[JsonConverter(typeof(GatewayJsonConverter))]
public abstract class Gateway
{
    /// <summary>
    /// The object ID of the gateway
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The type of the gateway
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// On-premises gateway
/// </summary>
public class OnPremisesGateway : Gateway
{
    /// <summary>
    /// The display name of the on-premises gateway
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The public key of the primary gateway member
    /// </summary>
    [JsonPropertyName("publicKey")]
    public PublicKey PublicKey { get; set; } = new();

    /// <summary>
    /// The version of the installed primary gateway member
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The number of gateway members in the on-premises gateway
    /// </summary>
    [JsonPropertyName("numberOfMemberGateways")]
    public int NumberOfMemberGateways { get; set; }

    /// <summary>
    /// The load balancing setting of the on-premises gateway
    /// </summary>
    [JsonPropertyName("loadBalancingSetting")]
    public string LoadBalancingSetting { get; set; } = string.Empty;

    /// <summary>
    /// Whether to allow cloud connections to refresh through this on-premises gateway
    /// </summary>
    [JsonPropertyName("allowCloudConnectionRefresh")]
    public bool AllowCloudConnectionRefresh { get; set; }

    /// <summary>
    /// Whether to allow custom connectors to be used with this on-premises gateway
    /// </summary>
    [JsonPropertyName("allowCustomConnectors")]
    public bool AllowCustomConnectors { get; set; }
}

/// <summary>
/// On-premises gateway (personal mode)
/// </summary>
public class OnPremisesGatewayPersonal : Gateway
{
    /// <summary>
    /// The public key of the gateway
    /// </summary>
    [JsonPropertyName("publicKey")]
    public PublicKey PublicKey { get; set; } = new();

    /// <summary>
    /// The version of the gateway
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Virtual network gateway
/// </summary>
public class VirtualNetworkGateway : Gateway
{
    /// <summary>
    /// The display name of the virtual network gateway
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The object ID of the Fabric license capacity
    /// </summary>
    [JsonPropertyName("capacityId")]
    public string CapacityId { get; set; } = string.Empty;

    /// <summary>
    /// The Azure virtual network resource
    /// </summary>
    [JsonPropertyName("virtualNetworkAzureResource")]
    public VirtualNetworkAzureResource VirtualNetworkAzureResource { get; set; } = new();

    /// <summary>
    /// The minutes of inactivity before the virtual network gateway goes into auto-sleep
    /// </summary>
    [JsonPropertyName("inactivityMinutesBeforeSleep")]
    public int InactivityMinutesBeforeSleep { get; set; }

    /// <summary>
    /// The number of member gateways
    /// </summary>
    [JsonPropertyName("numberOfMemberGateways")]
    public int NumberOfMemberGateways { get; set; }
}
