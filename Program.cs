using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace QuantumLottery
{


    // Reads random hex numbers form specified source, discards numbes which are out of range or duplicated
    // and makes lottery numbers for:
    // 1) The UK National lottery (number range: 1-59, items per line: 6)
    // 2) Eutomillions lottery (number range: 1-50, items per line: 5, bonus numbers 2, bonus range 1-12)
    class Program
    {

        static void PrintNationalLottery(int numLines = 4)
        {
            PrintLottoNums(numLines: numLines, 
                           debug: false);
        }

        static void PrintEuroMillions(int numLines = 2)
        {

            PrintLottoNums(minValue: 1, maxValue: 50,
                           itemsPerLine: 5, numLines: numLines,
                           bonusNumbers: 2, bonusMinValue: 1, bonusMaxValue: 12,
                           debug: false);
        }

        static void Main(string[] args)
        {
            // National lottery: default
            PrintNationalLottery(numLines: 1);

            // EuroMillions
            //PrintEuroMillions(numLines: 1);

            Console.ReadLine();
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
            int retryCount = 4;
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

            myClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36");

            int exceptionCounter = 0;

            while (lineNumber <= numLines)
            {
                byte myChar;
                string hexOutput = "";

                try
                {
                    hexOutput = myClient.DownloadString(randomHexUrl);
                }
                catch (Exception ex)
                {
                    if (exceptionCounter < retryCount)
                    {
                        if (debug) Console.WriteLine("Error retrieving: " + ex.Message);
                        // Allow the system to try once more
                        exceptionCounter++;
                    }
                    else
                    {
                        throw;
                    };
                };

                if (hexOutput.Length == 0)
                {
                    if (debug) Console.WriteLine("Empty response received, trying again");
                    continue;
                };

                myChar = Convert.ToByte(hexOutput, 16);

                if (lineItemCount == 1)
                {
                    currentMinValue = minValue;
                    currentMaxVlaue = maxValue;
                };

                if (myChar >= currentMinValue && myChar <= currentMaxVlaue)
                {

                    if (!Array.Exists(lotteryNums, lot => { return lot == myChar; }))
                    {
                        if (lineItemCount == 1)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Line Number: {0}", lineNumber);
                        };

                        if (bonusMode && lineItemCount == (itemsPerLine + 1))
                        {
                            Console.WriteLine();
                            Console.WriteLine("Bonus numbers:");
                            Console.WriteLine();
                        };

                        Console.WriteLine(myChar.ToString());

                        if (bonusMode)
                        {
                            // In bonus mode, we increment up to items per line + bonus items
                            lotteryNums[lineItemCount - itemsPerLine - 1] = myChar;
                        }
                        else
                        {
                            lotteryNums[lineItemCount - 1] = myChar;
                        };

                        lineItemCount++;

                        if (lineItemCount == (itemsPerLine + 1) &&
                            lineItemCount <= (itemsPerLine + bonusNumbers))
                        {
                            // Initialise for first bonus item
                            lotteryNums = new byte[bonusNumbers];
                            bonusMode = true;
                            currentMinValue = bonusMinValue;
                            currentMaxVlaue = bonusMaxValue;
                        }
                        else if (lineItemCount >= (itemsPerLine + 2) &&
                                 lineItemCount <= (itemsPerLine + bonusNumbers))
                        {
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
                    else if (debug)
                    {
                        Console.WriteLine("Duplicate: {0}", myChar.ToString());
                    };
                }
                else if (debug)
                {
                    Console.WriteLine("Outside range: {0}", myChar.ToString());
                };

            }
        }


    }
}
