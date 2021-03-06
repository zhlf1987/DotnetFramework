﻿using Dotnet.Utility;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Dotnet.Alg
{
    /// <summary>HMAC-SHA1加密
    /// </summary>
    public class HmacSha1Alg
    {
        /// <summary>获取HMAC-SHA1加密后的Base64值
        /// </summary>
        public static string GetStringBase64HmacSha1(string source, string key)
        {
            return GetStringBase64HmacSha1(source, key, "utf-8", "utf-8");
        }

        /// <summary>获取HMAC-SHA1加密后的Base64值
        /// </summary>
        public static string GetStringBase64HmacSha1(string source, string key, string sourceEncode, string keyEncode)
        {
            var hashBytes = GetHmacSha1(Encoding.GetEncoding(sourceEncode).GetBytes(source), Encoding.GetEncoding(keyEncode).GetBytes(key));
            return Convert.ToBase64String(hashBytes);
        }


        /// <summary>获取HMAC-SHA1加密后的十六进制值
        /// </summary>
        public static string GetStringHmacSha1(string source, string key, string sourceEncode, string keyEncode)
        {
            var hashBytes = GetHmacSha1(Encoding.GetEncoding(sourceEncode).GetBytes(source), Encoding.GetEncoding(keyEncode).GetBytes(key));
            return ByteBufferUtil.ByteArrayToString(hashBytes);
        }



        /// <summary>获取HMAC-SHA1加密后的
        /// </summary>
        public static byte[] GetHmacSha1(byte[] sourceBytes, byte[] keyBytes)
        {
            using (var hmacSha1 = new HMACSHA1(keyBytes))
            {
                var hashBytes = hmacSha1.ComputeHash(sourceBytes);
                return hashBytes;
            }
        }





    }
}
