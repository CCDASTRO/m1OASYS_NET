using System;

namespace m1OASYS_NET
{
    public static class Crc32
    {
        public static string CalculateCRC(string myData)
        {
            int myDataLen = myData.Length + 2;

            string lenStr;

            if (myDataLen < 10)
            {
                lenStr = "0" + myDataLen.ToString();
            }
            else
            {
                lenStr = myDataLen.ToString("X"); // matches VB6 HexNum behavior
            }

            string myData2 = lenStr + myData;

            int sum = 0;
            foreach (char c in myData2)
                sum += c;

            sum = sum % 256;

            int checksum = ((~sum + 1) & 0xFF);

            string strCRC = checksum.ToString("X2");

            return myData2 + strCRC + "\r\n";
        }
    }
}