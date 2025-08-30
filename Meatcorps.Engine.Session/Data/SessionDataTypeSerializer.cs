using System.Globalization;

namespace Meatcorps.Engine.Session.Data;

internal class SessionDataTypeSerializerInt : ISessionDataTypeSerializer
{
    public Type Type => typeof(int);

    public string Serialize(ISessionDataItem data)
    {
        if (data is not ISessionDataValue<int> v)
            throw new Exception("Invalid type");
        return v.Value.ToString(CultureInfo.InvariantCulture);
    }

    public void Deserialize(string value, ISessionDataItem data)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            throw new Exception("Invalid value for int: " + value);

        if (data is not ISessionDataValue<int> v)
            throw new Exception("Invalid type");
        v.Value = result;
    }
}

internal class SessionDataTypeSerializerFloat : ISessionDataTypeSerializer
{
    public Type Type => typeof(float);

    public string Serialize(ISessionDataItem data)
    {
        if (data is not ISessionDataValue<float> v)
            throw new Exception("Invalid type");
        return v.Value.ToString(CultureInfo.InvariantCulture);
    }

    public void Deserialize(string value, ISessionDataItem data)
    {
        if (!float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
            throw new Exception("Invalid value for float: " + value);

        if (data is not ISessionDataValue<float> v)
            throw new Exception("Invalid type");
        v.Value = result;
    }
}

internal class SessionDataTypeSerializerString : ISessionDataTypeSerializer
{
    public Type Type => typeof(string);

    public string Serialize(ISessionDataItem data)
    {
        if (data is not ISessionDataValue<string> v)
            throw new Exception("Invalid type");
        return v.Value ?? string.Empty;
    }

    public void Deserialize(string value, ISessionDataItem data)
    {
        if (data is not ISessionDataValue<string> v)
            throw new Exception("Invalid type");
        v.Value = value ?? string.Empty;
    }
}

public interface ISessionDataTypeSerializer
{
    public Type Type { get; }
    public string Serialize(ISessionDataItem data);
    public void Deserialize(string value, ISessionDataItem data);
}