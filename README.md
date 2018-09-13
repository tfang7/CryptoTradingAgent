# CryptoTradingAgent
Assignment for Crypto Commonwealth Interview

Uses KucoinAPI to obtain current cryptocurrency prices of BTC, ETH, BCH, NEO.

Simple trading strategy.
  Buying -> If price is lowest observed by agent so far or program has just started then buy in.
  Selling -> If price of a coin has changed enough from buy-in price and selling creates profit > 0, then dump all coins.
  
  
