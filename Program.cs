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
            // National lottery: default
            PrintLottoNums();

            // EuroMillions
            //PrintLottoNums(minValue: 1, maxValue: 50,
            //               itemsPerLine: 5, numLines: 2,
            //               bonusNumbers: 2, bonusMinValue: 1, bonusMaxValue: 12, 
            //               debug: false);
        }
        
        static void PrintLottoNums(int minValue = 1, 
                                   int maxValue = 59, 
                                   int itemsPerLine = 6,
                                   int numLines = 4,
                                   int bonusNumbers = 0,
                                   int bonusMinValue = 1,
                                   int bonusMaxValue = 12,
                                   bool debug = false,
                                   string randomHexUrl = "https://qrng.anu.edu.au/ran_hex.php")
        {
            int itemsPerLineWithBonus = itemsPerLine + bonusNumbers;
            byte[] lotteryNums = new byte[itemsPerLine];
            int lineItemCount = 1;
            int lineNumber = 1;

            int currentMinValue = 0;
            int currentMaxVlaue = 0;

            // Set to true when generating bonus balls
            bool bonusMode = false;

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

                if (lineItemCount == 1) {
                    currentMinValue = minValue;
                    currentMaxVlaue = maxValue;
                };

                if (myChar >= currentMinValue && myChar <= currentMaxVlaue) {

                    if (!Array.Exists(lotteryNums, (byte lot) => { return lot == myChar; } )) {

                        if (lineItemCount == 1) {
                            Console.WriteLine();
                            Console.WriteLine("Line Number: {0}", lineNumber);
                        };

                        if (bonusMode && lineItemCount == (itemsPerLine + 1)) {
                            Console.WriteLine();
                            Console.WriteLine("Bonus numbers:");
                            Console.WriteLine();
                        };

                        Console.WriteLine(myChar.ToString());

                        if (bonusMode) {
                            // In bonus mode, we increment up to items per line + bonus items
                            lotteryNums[lineItemCount - itemsPerLine - 1] = myChar;
                        }
                        else {
                            lotteryNums[lineItemCount - 1] = myChar;
                        };

                        lineItemCount++;

                        if (lineItemCount == (itemsPerLine + 1) &&
                            lineItemCount <= (itemsPerLine + bonusNumbers) ) {
                            // Initialise for first bonus item
                            lotteryNums = new byte[bonusNumbers];
                            bonusMode = true;
                            currentMinValue = bonusMinValue;
                            currentMaxVlaue = bonusMaxValue;
                        }
                        else if (lineItemCount >= (itemsPerLine + 2) && 
                                 lineItemCount <= (itemsPerLine + bonusNumbers) ) {
                            // All the rest of the nbonus numebrs

                        }
                        else if (lineItemCount > itemsPerLineWithBonus)
                        {
                            lotteryNums = new byte[itemsPerLine];
                            lineItemCount = 1;
                            lineNumber++;
                            bonusMode = false;
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
