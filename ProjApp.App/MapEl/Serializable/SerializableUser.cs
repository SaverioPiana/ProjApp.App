using Microsoft.Maui.Devices.Sensors;

public class SerializableUser
{
    public string Nickname { get; set; }
    public string UserID { get; set; }
    public Location Position { get; set; }
    public byte[] UserIcon { get; set; }
    public bool IsCercatore { get; set; }
    public bool IsPreso { get; set; }
    public bool IsSalvo { get; set;}
}