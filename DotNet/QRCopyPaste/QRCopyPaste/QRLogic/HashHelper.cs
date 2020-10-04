﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace QRCopyPaste
{
    public static class HashHelper
    {
        public static string GetStringHash(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr))
                return string.Empty;

            var dataBytes = Encoding.UTF8.GetBytes(dataStr);
            using var sha = new SHA256Managed();
            var hashBytes = sha.ComputeHash(dataBytes);
            var hashStr = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            return hashStr;
        }
    }
}
