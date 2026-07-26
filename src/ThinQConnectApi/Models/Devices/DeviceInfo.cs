namespace ThinQConnectApi.Models.Devices;

public class DeviceInfo
{
    public required string DeviceType { get; set; }
    
    public required string ModelName { get; set; }
    
    public required string Alias { get; set; }
    
    public bool Reportable  { get; set; }
}