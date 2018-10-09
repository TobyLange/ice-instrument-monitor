ICE Instrument Monitoring Coding Exercise

Toby Lange (tobywlange@gmail.com)

Assumptions:
  - Tickers are uppercase.
  - Tickers are limited to 35 characters.
  - Prices are displayed with a maximum of 4 decimal places.
  - Ticks are published in UTC and displayed in local time.

Limitations:
  - The ListView displaying the market data will not scale. For that you would need to use a 'virtual' ListView implementation.
  - The columns are sortable, but the column headers do not indicate the current state.
  - The SingleSourceEngine generates new price data for each ticker every 500 ms. 
