using System.Text;
using System.Runtime;
using System.Security.Cryptography;
using SleepysToolbox.Models;

namespace SleepysToolbox.JwtHelpers
{
    public static class CryptoHelper
    {
        public static HashWithSaltResult HashPW(string pw)
        {
            return new PasswordWithSaltHasher().HashWithSalt(pw, 64, SHA256.Create());
        }

        public static HashWithSaltResult HashPW(string pw, string salt)
        {
            return new PasswordWithSaltHasher().HashWithSalt(pw, salt, SHA256.Create());
        }

        public static bool VerifyHashedPW(Users user, string pw)
        {
            var hashedPw = new PasswordWithSaltHasher().HashWithSalt(pw, user.Password.Split(':')[1], SHA256.Create());
            return hashedPw.Digest.Equals(user.Password.Split(':')[0]);
        }
    }

    public class HashWithSaltResult
    {
        public string Salt { get; }
        public string Digest { get; set; }

        public string Combined { get { return String.Join(':', new string[] { Digest, Salt }); } }

        public HashWithSaltResult(string salt, string digest)
        {
            Salt = salt;
            Digest = digest;
        }
    }

    public class PasswordWithSaltHasher
    {
        public HashWithSaltResult HashWithSalt(string password, int saltLength, HashAlgorithm hashAlgo)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(saltLength);
            byte[] passwordAsBytes = Encoding.UTF8.GetBytes(password);
            List<byte> passwordWithSaltBytes = new List<byte>();
            passwordWithSaltBytes.AddRange(passwordAsBytes);
            passwordWithSaltBytes.AddRange(saltBytes);
            byte[] digestBytes = hashAlgo.ComputeHash(passwordWithSaltBytes.ToArray());
            return new HashWithSaltResult(Convert.ToBase64String(saltBytes), Convert.ToBase64String(digestBytes));
        }

        public HashWithSaltResult HashWithSalt(string password, string salt, HashAlgorithm hashAlgo)
        {
            byte[] passwordAsBytes = Encoding.UTF8.GetBytes(password);
            List<byte> passwordWithSaltBytes = new List<byte>();
            passwordWithSaltBytes.AddRange(passwordAsBytes);
            passwordWithSaltBytes.AddRange(Convert.FromBase64String(salt));
            byte[] digestBytes = hashAlgo.ComputeHash(passwordWithSaltBytes.ToArray());
            return new HashWithSaltResult(salt, Convert.ToBase64String(digestBytes));
        }
    }
}
