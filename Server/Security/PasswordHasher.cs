using System.Security.Cryptography;

namespace Server.Security;

// PBKDF2-SHA256 có salt. Định dạng lưu: v1.{iterations}.{saltBase64}.{hashBase64}
public class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 120_000;

    // fake hashing -> thời gian phản hồi gần như nhau -> avoid side-chanel attack
    private static readonly string DummyHash = new PasswordHasher().Hash("dummy-password");

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string stored)
    {
        try
        {
            var parts = stored.Split('.');
            if (parts.Length != 4 || parts[0] != "v1") return false;

            int iterations = int.Parse(parts[1]);
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);

            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
        {
            return false;
        }
    }

    public void VerifyDummy(string password) => Verify(password, DummyHash);
}
