using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrading
{
    class AgentCoin
    {
        public String type;
        // Average price paid per coin
        private double totalValue;
        // Collection of prices gathered
        private Queue<double> pricePoints;
        private const int numPrices = 100;

        public double lastDealPrice;
        public double buyPrice;
        public double numOwned;

        // Constructor for an agent coin object
        public AgentCoin(string coinType)
        {
            type = coinType;
            numOwned = 0;
            totalValue = 0.0d;
            lastDealPrice = 0;
            buyPrice = 0;
            pricePoints = new Queue<double>();
        }
        public double getTotalValue()
        {
            return numOwned * buyPrice;
        }
        public void setTotalValue(double val)
        {
            totalValue = val;
        }
        // Keeps track of 100 prices
        // When the queue is at max size, the queue pops off the oldest element
        // adds the newest element at the end
        public void UpdatePrice(double tickPrice)
        {
            lastDealPrice = tickPrice;
            if (pricePoints.Count > numPrices)
            {
                pricePoints.Dequeue();
            }
            if (pricePoints.Count == 0 || tickPrice - pricePoints.Peek() != 0)
            {
                pricePoints.Enqueue(tickPrice);
            }

        }
        public double getLowestPriceSeen()
        {
            double min = double.MaxValue;
            foreach (double price in pricePoints)
            {
                if (price < min)
                {
                    min = price;
                }
            }
            return min;
        }
    }

}
