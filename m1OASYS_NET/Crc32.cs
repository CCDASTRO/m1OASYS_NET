using System;

namespace m1OASYS_NET
{
    public static class Crc32
    {
        public static string CalculateCRC(string data)
        {
            int sum = 0;
            string full = data;

            for (int i = 0; i < full.Length; i++)
                sum += (byte)full[i];

            int crc = ((~sum + 1) & 0xFF);

            return full + crc.ToString("X2") + "\r\n";
        }
    }
}