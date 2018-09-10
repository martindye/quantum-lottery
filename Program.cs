using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace QuantumLottery
{

    // Reads random hex numbers form specified source, discards numbes which are out of range or duplicated
    // and makes lottery numbers for the UK National lottery (number range: 1-59, items per line: 6)
    class Program
    {

        static void Main(string[] args)
        {
            // Test
            PrintLottoNums();
        }
        
        static void PrintLottoNums()
        {
        
            // Options
            string randomHexUrl = "https://qrng.anu.edu.au/ran_hex.php";
            bool debug = false;

            int minValue = 1;
            int maxValue = 59;
            int itemsPerLine = 6;
            int numLines = 4;

            byte[] lotteryNums = new byte[itemsPerLine];
            int lineItemCount = 1;
            int lineNumber = 1;

            // Confirmed: server sends out 2-digit hex numbers at a time

            // WebRequest.DefaultWebProxy = null;
            WebClient myClient = new WebClient();
           
            // Fiddler
            // myClient.Proxy = new WebProxy("http://localhost:8888");

            // Ensure no proxy
            myClient.Proxy = null;
            Uri randomHexUri = new Uri(randomHexUrl);

            while (lineNumber <= numLines)
            {
                byte myChar;
                string hexOutput = myClient.DownloadString(randomHexUrl);
                
                myChar = Convert.ToByte(hexOutput, 16);

                if (myChar >= minValue && myChar <= maxValue) {

                    if (!Array.Exists(lotteryNums, (byte lot) => { return lot == myChar; } )) {

                        if (lineItemCount == 1) {
                            Console.WriteLine();
                            Console.WriteLine("Line Number: {0}", lineNumber);
                        };

                        Console.WriteLine(myChar.ToString());
                        lotteryNums[lineItemCount - 1] = myChar;
                        lineItemCount++;

                        if (lineItemCount > itemsPerLine) {
                            lineItemCount = 1;
                            lineNumber++;
                            lotteryNums = new byte[itemsPerLine];
                        };

                    }
                    else if (debug) {
                        Console.WriteLine("Duplicate: {0}", myChar.ToString());
                    };
                }
                else if (debug) {
                    Console.WriteLine("Outside range: {0}", myChar.ToString());
                };
                
            }
        }
    }
}
