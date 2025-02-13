namespace BookwormsServer.Utils;

public static class Snowflake
{
    // Midnight, Jan 1st, 2025 (MST)
    private const long EpochStartMilli = 1735714800000L;
    
    /// <summary>
    /// Generates a unique ID based on the current time and a random number.
    /// IDs are formated as hex strings, and are about 14 digits long, timestamp depending.
    /// Loosely based of the snowflake algorithm used by Twitter, Discord, and others.
    /// </summary>
    /// <returns>A new unique ID</returns>
    public static string Generate()
    {
        // 41 bits for time
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long milliTimeSinceEpoch = currentTime - EpochStartMilli & 0x1FFFFFFFFFF;
        
        // 22 bits for randomly generated number
        long randomNum = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0x3FFFFF);

        // First bit is unused to prevent sign issues
        long id = milliTimeSinceEpoch << 22 | randomNum;
        return Convert.ToString(id, 16);
    }
}