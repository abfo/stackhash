using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace StackHashUtilities
{
    public static class SecurityUtils
    {
        /// <summary>
        /// This produces a hex string of the following format.
        /// 12-AB-99- etc...
        /// </summary>
        /// <param name="plaintext">Plaintext string.</param>
        /// <returns>Encrypted string</returns>
        public static String EncryptStringWithUserCredentials(String plaintext)
        {
            if (String.IsNullOrEmpty(plaintext))
                return String.Empty;

            // Get the byte representation of the string (UTF8 so that there is not a 0 for the high unicode bytes).
            Byte[] stringInBytes = Encoding.UTF8.GetBytes(plaintext);

            if (stringInBytes.Length == 0)
	            return String.Empty;

            byte[] entropy = { 0x12, 0x34, 0x56, 0x78 };
            byte[] encryptedData = ProtectedData.Protect(stringInBytes, entropy, DataProtectionScope.LocalMachine);
            
            // Convert to a hex string
            return ByteArrayToHexString(encryptedData);
        }


        /// <summary>
        /// Decrypts the specified string - must be in hex format thus...
        /// 12-23-A5 etc...
        /// </summary>
        /// <param name="encryptedText">Encrypted hex string</param>
        /// <returns>Decrypted string.</returns>
        public static String DecryptStringWithUserCredentials(String encryptedText)
        {
            if (String.IsNullOrEmpty(encryptedText))
                return String.Empty;

            // Convert the hex string to a byte array.
            Byte[] stringInBytes = HexStringToByteArray(encryptedText);

            byte[] entropy = { 0x12, 0x34, 0x56, 0x78 };
            byte[] decryptedData = ProtectedData.Unprotect(stringInBytes, entropy, DataProtectionScope.LocalMachine);

            return Encoding.UTF8.GetString(decryptedData);
        }


        /// <summary>
        /// Converts a byte array into a hex string of the format AB-01-32 etc...
        /// </summary>
        /// <param name="byteArray">The byte array to convert.</param>
        /// <returns>Hex string representation of the byte array.</returns>
        public static string ByteArrayToHexString(byte[] byteArray)
        {
            String hex = BitConverter.ToString(byteArray);
            return hex;
        }

        /// <summary>
        /// Converts a hex string representation of an array to a byte array.
        /// The hex string is in the form AB-12-23 etc...
        /// </summary>
        /// <param name="hexText">Hex string representation of the array.</param>
        /// <returns>Byte array representing the data.</returns>
        public static byte[] HexStringToByteArray(String hexText)
        {
            if (hexText == null)
                return null;

            int NumberChars = hexText.Length;
            byte[] bytes = new byte[NumberChars / 2];
            int byteIndex = 0;

            // The format of the string is AB-12- etc... so 3 chars per value.
            for (int i = 0; i < NumberChars; i += 3)
            {
                bytes[byteIndex++] = Convert.ToByte(hexText.Substring(i, 2), 16);
            }
            return bytes;
        } 
    }
}
