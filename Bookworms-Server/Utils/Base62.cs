namespace BookwormsServer.Utils;

public class Base62
{
    private const string Base62Values = "0123456789" +
                                        "ABCDEFGHIJKLMNOPQRSTUVWXYZ"+
                                        "abcdefghijklmnopqrstuvwxyz";
        
    public static string FromGuid(Guid guid)
    {
        var bytes = guid.ToByteArray();
        UInt128 num = UInt128Ext.FromBytes(bytes);
        
        string base62Result = "";
        if (num == 0)
        {
            base62Result = Base62Values[0].ToString();
        }
        while (num > 0)
        {
            UInt128 mod = num % 62;
            int r = (int)mod;
            base62Result = Base62Values[r] + base62Result;
            num /= 62;
        }

        return base62Result.PadLeft(22, '0');
    }

    public static Guid ToGuid(string base62Input)
    {
        UInt128 result = 0;
        uint digit = 0;
        
        foreach (char c in base62Input.Reverse())
        {
            UInt128 numIndex = UInt128.CreateChecked(Base62Values.IndexOf(c));
            result += numIndex * UInt128Ext.Pow(62, digit);
            digit++;
        }

        byte[] bytesOutput = result.ToBytes();
        return new(bytesOutput);
    }
}

public static class UInt128Ext
{
    public static UInt128 FromBytes(byte[] bytes)
    {
        UInt64 upper = BitConverter.ToUInt64(bytes);
        UInt64 lower = BitConverter.ToUInt64(bytes, 8);

        return new(upper, lower);
    }

    public static byte[] ToBytes(this UInt128 num)
    {
        UInt64 downOut = (UInt64) (num & UInt64.MaxValue);
        UInt64 upOut = (UInt64) (num >> 64);
        
        byte[] upOutBytes = BitConverter.GetBytes(upOut);
        byte[] downOutBytes = BitConverter.GetBytes(downOut);
        
        byte[] bytesOutput = new byte[16];
        Array.Copy(upOutBytes, bytesOutput, 8);
        Array.Copy(downOutBytes, 0, bytesOutput, 8, 8);

        return bytesOutput;
    }

    public static UInt128 Pow(this UInt128 b, uint exp)
    {
        UInt128 result = 1;
        for (int i = 0; i < exp; i++)
        {
            result *= b;
        }

        return result;
    }
}