using System;
using Xunit;
using CryptoTrading;
using System.Collections.Generic;

namespace XUnitTestProject
{
    public class UnitTest
    {
        // Test if agent parameters are working properly.
        [Fact]
        public void TestAgentCreationValidArguments()
        {
            List<String> Coins = new List<String> { "ETH", "BTC", "BCH", "NEO" };
            TradingAgent agent = new TradingAgent(100.0d, Coins);
            Assert.NotNull(agent);
        }
        [Fact]
        public void TestAgentCreationInvalidArguments()
        {
            List<String> Coins = new List<String> { "ETH", "BTC", "BCH", "NEO" };
            TradingAgent agent = new TradingAgent(-100.0d, Coins);
        }
        // Test if Coin Type input matches values in agent.
        [Fact]
        public void TestAgentCoinsInput()
        {
            List<String> Coins = new List<String> { "ETH", "BTC", "BCH", "NEO" };
            TradingAgent agent = new TradingAgent(100.0d, Coins);
            List<String> agentCoins = agent.GetCoinTypes();
            Assert.NotEmpty(agentCoins);

            Coins = new List<String> { };
            agent = new TradingAgent(100.0d, Coins);
            agentCoins = agent.GetCoinTypes();
            Assert.Empty(agentCoins);
        }
        // Test if initialized buying power is equal to value of current agent buying power
        [Fact]
        public void TestBuyingPower()
        {
            List<String> Coins = new List<String> { "ETH", "BTC", "BCH", "NEO" };
            double testBuyingPower = 100.0d;
            TradingAgent agent = new TradingAgent(testBuyingPower, Coins);
            List<String> agentCoins = agent.GetCoinTypes();

            Assert.Equal(testBuyingPower, agent.GetBuyingPower());
        }
        // Test if kucoin client was successfully created
        [Fact]
        public void TestKucoinClient()
        {
            List<String> coins = new List<String> { "ETH", "BTC", "BCH", "NEO" };
            TradingAgent agent = new TradingAgent(100.0d, coins);
            KuCoinApi.NetCore.KuCoinApiClient kucoinClient = agent.GetKucoinClient();
            Assert.NotNull(kucoinClient);
        }

        // Test whether available funds where used to carry out a transaction.
        [Fact]
        public void TestDecideTransaction()
        {
            List<String> coins = new List<String> { "ETH", "BTC", "BCH", "NEO" };
            TradingAgent agent = new TradingAgent(100.0d, coins);

            KuCoinApi.NetCore.KuCoinApiClient kucoinClient = agent.GetKucoinClient();
            KuCoinApi.NetCore.Entities.Tick[]  ticks = kucoinClient.GetTicks();

            agent.GetCoinInfo(ticks, coins);
            agent.DecideTransaction();
            // Buying power has changed due to buy transaction being carried out.
            Assert.NotEqual(100.0d, agent.GetBuyingPower());
        }
    }
}
