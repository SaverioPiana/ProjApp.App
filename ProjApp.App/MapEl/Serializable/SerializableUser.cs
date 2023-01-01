using Microsoft.Maui.Devices.Sensors;

public class SerializableUser
{
    public string Nickname { get; set; }
    public string UserID { get; set; }
    public Location Position { get; set; }
    public byte[] UserIcon { get; set; }
}