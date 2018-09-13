using System;
using KuCoinApi.NetCore;
using System.Threading;
using System.Collections.Generic;

namespace CryptoTrading
{
    class TradingAgent
    {
        // Number of arguments for agent update thread.
        private const int numArgs = 2;

        //Initial starting cash that agent has to work with.
        private static double totalBuyingPower = 1000000.0d;
        private static double availableBuyingPower;

        //  dictionary agentCoins where key is string representing the coin and the value is a cryptocoin object.
        // ex: "ETH" : Ethereum Cryptocoin object
        private static Dictionary<string, AgentCoin> agentCoins;

        static void Main(string[] args)
        {
            Console.WriteLine("Beginning Crypto Trading Agent Program...");
            Init();
        }
        private static void Init()
        {
            var kucoinClient = new KuCoinApiClient();
            List<String> Coins = new List<String> { "ETH", "BTC", "BCH", "NEO" };

            // Sets available buying power equal to total initially
            InitBuyingPower();

            // Sets up crypto coin disctionary
            InitAgentCoins(Coins);

            Thread mainThread = new Thread(new ParameterizedThreadStart(AgentStep));
            object threadArgs = new object[numArgs] { kucoinClient, Coins };

            Console.WriteLine("Starting new thread");
            mainThread.Start(threadArgs);
            Console.ReadLine();
        }
        // Input 0: List of coin types as strings
        // Dictionary can be of variable length dependent on user input
        // Input 0: kucoin client for obtaining exchange rates, 1: String List of coins to search for.
        private static void AgentStep(object threadArgs)
        {
            Array targArray = (Array) threadArgs;
            
            //kucoin client parameters
            KuCoinApiClient kucoinClient = (KuCoinApiClient) targArray.GetValue(0);
            KuCoinApi.NetCore.Entities.Tick[] Ticks;

            //list of crypto-coins
            List<String> Coins = (List<String>) targArray.GetValue(1);

            /* bot checks realtime data every x mins/seconds*/
            // Currently set to update every 10 seconds
            int seconds = 10000, minutes = 1, hours = 1;

            //Main update loop
            while (true)
            {
                Ticks = kucoinClient.GetTicks();
                if (Ticks == null) throw new ArgumentNullException("ERR: Ticks received from KucoinAPI is null");
                
                Console.WriteLine(GetCoinInfo(Ticks, Coins));
                DecideTransaction();
                Thread.Sleep(seconds * minutes * hours);
            }

        }
        // Agent Trading Strategy
        // In the beginning, just enter the market
        // Then only buy amount of crypto coins equal to ratios determined by user. 
        // For simplicity, this agent just spends 1/4th of buying power for each coin.
        // and only buys when the price is the lowest the agent has observed since running.
        // Sell coins as soon as profit can be made.
        private static void DecideTransaction()
        {
            int transactionAmount = 0;
            double transactionValue = 0;
            double profit;
            // diversity, spend equal amounts of cash for each coin
            double ratio = totalBuyingPower / agentCoins.Count;

            //Loop through each coin and decide to sell or buy a coin
            foreach (AgentCoin coin in agentCoins.Values)
            {
                if (coin == null) throw new ArgumentNullException("ERR: Coin object agentCoin dict is null");
                transactionAmount = 0;
                transactionValue = 0;

                if (coin.getTotalValue() == 0 && coin.lastDealPrice <= coin.getLowestPriceSeen())
                {
                    // Increment amount of coins until value of amount to be bought will be just under the diversity ratio
                    while (coin.lastDealPrice + transactionValue < ratio)
                    {
                        transactionValue += coin.lastDealPrice;
                        transactionAmount++;
                    }
                    Console.WriteLine("Buy " + transactionAmount + " " + coin.type + " at $" + transactionValue);

                    coin.buyPrice = coin.lastDealPrice;
                    coin.numOwned += transactionAmount;

                    availableBuyingPower -= transactionValue;
                }
                // Else consider amount of profit made from selling a coin
                // Dump all coins when desired amount of profit can be made.
                else
                {
                    // Currently sells as soon as it is profitable
                    // i.e. when the lastDealPrice is greater than the buy in price.
                    transactionValue = 0.0d;
                    bool isProfitable = coin.lastDealPrice - coin.buyPrice > 0.0d;
                    if (coin.numOwned > 0 && isProfitable)
                    {
                        profit = coin.lastDealPrice * coin.numOwned;
                        Console.WriteLine("Sell " + coin.numOwned + " " + coin.type + " at $" + transactionValue);

                        availableBuyingPower += transactionValue;
                        coin.numOwned = 0;
                    }
                }
            }
        }
        // Uses KuCoin API to obtain tick info and display in formatted string.
        private static String GetCoinInfo(KuCoinApi.NetCore.Entities.Tick[] Ticks, List<String> Coins)
        {
            String tickInfo = InitTickInfo();
            foreach (var tick in Ticks)
            {
                if (tick.coinTypePair != "USDT") continue;

                if (Coins.Contains(tick.coinType))
                {
                    tickInfo += (tick.coinType + "\t\t" + tick.lastDealPrice + '\n');
                    agentCoins[tick.coinType].UpdatePrice((double)tick.lastDealPrice);
                }

            }
            tickInfo += ("------------------------------------\n");
            return tickInfo;
        }
        private static String InitTickInfo()
        {
            String str = (DateTime.Now + "\n");
            str += ("Coin Type" + "\t" + "Last Deal Price\n");
            str += ("------------------------------------\n");
            return str;
        }
        private static void InitAgentCoins(List<String> Coins)
        {
            agentCoins = new Dictionary<string, AgentCoin>();

            for (int i = 0; i < Coins.Count; i++)
            {
                string coinType = Coins[i];
                agentCoins[coinType] = new AgentCoin(Coins[i]);
            }
        }
        private static void InitBuyingPower()
        {
            if (totalBuyingPower <= 0)
            {
                throw new ArgumentOutOfRangeException("totalBuyingPower", totalBuyingPower, "ERR: Insufficient Buying Power to carry out transactions");
            }
            availableBuyingPower = totalBuyingPower;
        }

    }
}
