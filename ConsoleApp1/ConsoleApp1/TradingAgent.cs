using System;
using KuCoinApi.NetCore;
using System.Threading;
using System.Collections.Generic;

namespace CryptoTrading
{
    public class TradingAgent
    {
        // Number of arguments for agent update thread.
        private const int numArgs = 2;

        //Initial starting cash that agent has to work with.
        private static double totalBuyingPower = 0.0d;
        private static double availableBuyingPower;

        //  dictionary agentCoins where key is string representing the coin and the value is a cryptocoin object.
        // ex: "ETH" : Ethereum Cryptocoin object
        private static Dictionary<string, AgentCoin> agentCoins = null;
        private static List<String> Coins = null;
        private static KuCoinApiClient kucoinClient = null;

        public TradingAgent(double buyingPower, List<String> inputCoins)
        {
            Init(buyingPower, inputCoins);
            Console.WriteLine("Beginning Crypto Trading Agent Program...");
        }
        public double GetBuyingPower()
        {
            return availableBuyingPower;
        }
        public List<String> GetCoinTypes()
        {
            return Coins;
        }
        public KuCoinApiClient GetKucoinClient()
        {
            return kucoinClient;
        }
        private static void Init(double buyingPower, List<String> inputCoins)
        {
            Coins = inputCoins;
            // Sets available buying power equal to total initially
            totalBuyingPower = buyingPower;
            kucoinClient = new KuCoinApiClient();
            // Sets up crypto coin disctionary
            InitAgentCoins(Coins);
            InitBuyingPower();

        }
        // Agent Trading Strategy
        // In the beginning, just enter the market
        // Then only buy amount of crypto coins equal to ratios determined by user. 
        // For simplicity, this agent just spends 1/4th of buying power for each coin.
        // and only buys when the price is the lowest the agent has observed since running.
        // Sell coins as soon as profit can be made.
        public void DecideTransaction()
        {
            double transactionAmount = 0;
            double transactionValue = 0;
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
                    BuyCoin(coin, transactionAmount);
                }
                // Else consider amount of profit made from selling a coin
                // Dump all coins when desired amount of profit can be made.
                else
                {
                    // Currently sells as soon as it is profitable
                    // i.e. when the lastDealPrice is greater than the buy in price.
                    bool isProfitable = coin.lastDealPrice - coin.buyPrice > 0.0d;
                    if (coin.numOwned > 0 && isProfitable)
                    {
                        transactionAmount = coin.numOwned;
                        SellCoin(coin, transactionAmount);
                    }
                }
            }
        }
        private void BuyCoin(AgentCoin coin, double amount)
        {
            double transactionValue;

            coin.buyPrice = coin.lastDealPrice;
            coin.numOwned += amount;
            transactionValue = amount * coin.buyPrice;

            availableBuyingPower -= transactionValue;
            Console.WriteLine("Buy " + amount + " " + coin.type + " at $" + transactionValue);

        }
        private void SellCoin(AgentCoin coin, double amount)
        {
            double transactionValue;
            transactionValue = coin.lastDealPrice * amount;
            
            availableBuyingPower += transactionValue;
            coin.numOwned -= amount;

            Console.WriteLine("Sell " + amount + " " + coin.type + " at $" + transactionValue);
        }
        // Uses KuCoin API to obtain tick info and display in formatted string.
        public String GetCoinInfo(KuCoinApi.NetCore.Entities.Tick[] Ticks, List<String> Coins)
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
            availableBuyingPower = totalBuyingPower;
        }

    }
}
