using Mapsui.UI.Maui;

public class SerializablePin
{
    public int Type { get; set; }
    public Position Position { get; set; }
    public float Scale { get; set; }
    public Color Color { get; set; }
    public string Label { get; set; }
    public object Address { get; set; }
    public byte[] Icon { get; set; }
    public object Svg { get; set; }
    public int Rotation { get; set; }
    public bool RotateWithMap { get; set; }
    public bool IsVisible { get; set; }
    public int MinVisible { get; set; }
    public double MaxVisible { get; set; }
    public Anchor Anchor { get; set; }
    public int Transparency { get; set; }
    public object Tag { get; set; }
    public object BindingContext { get; set; }
}
public class Anchor
{
    public int X { get; set; }
    public int Y { get; set; }
}

