using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Milestone_2
{
    public class HashKey
    {
        public HashKey()
        {

        }

        // Returns the MD5 hash of any given string
        public string GetPasswordHash(string password)
        {
            using (MD5 hash = MD5.Create())
            {
                return GetMd5Hash(hash, password);
            }
        }

        // Source:
        // https://msdn.microsoft.com/en-us/library/system.security.cryptography.md5(v=vs.110).aspx
        private string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // This method generates a random
        // word of length 22 (arbitrary length)
        // and gets the MD5 hash key of it.
        public string GetRandomMD5HashValue()
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            string randomWord = new string(Enumerable.Repeat(chars, 22)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return GetPasswordHash(randomWord);
        }
    }
}
