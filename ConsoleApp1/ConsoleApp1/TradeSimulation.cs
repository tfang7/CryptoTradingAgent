using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CryptoTrading
{
    class TradeSimulation
    {
        private static TradingAgent agent;
        private static void Main(string[] args)
        {
            RunSimulation();
        }
        private static void RunSimulation()
        {
            List<String> coins = new List<String> { "ETH", "BTC", "BCH", "NEO" };
            agent = new TradingAgent(100000.0d, coins);
            KuCoinApi.NetCore.KuCoinApiClient kucoinClient = agent.GetKucoinClient();

            //Main update loop
            while (true)
            {
                /* bot checks realtime data every x mins/seconds
                   Currently set to update every 10 seconds */
                Update(kucoinClient, coins);
            }

        }
        // Input 0: kucoin client for obtaining exchange rates, 1: String List of coins to search for.
        private static void Update(KuCoinApi.NetCore.KuCoinApiClient kucoinClient, List<String> coins)
        {
            int seconds = 10000, minutes = 1, hours = 1;
            KuCoinApi.NetCore.Entities.Tick[] ticks;

            ticks = kucoinClient.GetTicks();
            if (ticks == null) throw new ArgumentNullException("ERR: Ticks received from KucoinAPI is null");

            Console.WriteLine(agent.GetCoinInfo(ticks, coins));
            agent.DecideTransaction();

            Thread.Sleep(seconds * minutes * hours);
        }
    }
}
