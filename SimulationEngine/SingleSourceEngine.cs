using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SimulationEngine
{
    public class SingleSourceEngine : IPriceEngine
    {
        private readonly ConcurrentDictionary<string, SimulationData> _prices = new ConcurrentDictionary<string, SimulationData>();
        private Timer _timer;

        private class SimulationData
        {
            public decimal Price { get; private set; }
            private decimal _lowerBound;
            private decimal _upperBound;
            private decimal _stride;
            private decimal _direction;
            private Random _random = new Random();

            public SimulationData()
            {
                _lowerBound = RandomBetween(20m, 40m);
                _upperBound = RandomBetween(_lowerBound, _lowerBound + 10m);
                _stride = RandomBetween(0m, 0.2m);
                _direction = _random.NextDouble() > 0.5 ? 1m : -1m;
                Price = (_lowerBound + _upperBound) / 2m;
            }

            public void GenerateNextPrice()
            {
                Price += RandomBetween(0m, _stride) * _direction;
                if (_direction > 0)
                {
                    if (Price > _upperBound)
                    {
                        _direction = -1m;
                    }
                }
                else
                {
                    if (Price < _lowerBound)
                    {
                        _direction = 1m;
                    }
                }
            }

            private decimal RandomBetween(decimal lowerBound, decimal upperBound)
            {
                decimal rnd = (decimal)_random.NextDouble();

                return lowerBound + (rnd * (upperBound - lowerBound));
            }
        }

        public class MarketData : IMarketData
        {
            public string Ticker { get; set; }
            public decimal Price { get; set; }
            public DateTime TicksUTC { get; set; }

        }

        public bool IsStarted => _timer != null;

        public ConcurrentQueue<IMarketData> Feed { get; } = new ConcurrentQueue<IMarketData>();

        public void Start()
        {
            if (_timer == null)
            {
                _timer = new Timer(Tick, null, 500, 500);
            }
        }

        private void Tick(object state)
        {
            foreach (var pair in _prices.ToArray())
            {
                pair.Value.GenerateNextPrice();
                var data = new MarketData { Ticker = pair.Key, Price = pair.Value.Price, TicksUTC = DateTime.UtcNow };
                Feed.Enqueue(data);
            }
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Subscribe(string ticker)
        {
            _prices.GetOrAdd(ticker, new SimulationData());
        }

        public void Unsubscribe(string ticker)
        {
            SimulationData data;
            _prices.TryRemove(ticker, out data);
        }
    }
}
