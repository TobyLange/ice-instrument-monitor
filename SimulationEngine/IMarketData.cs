using System;

namespace SimulationEngine
{
    public interface IMarketData
    {
        string Ticker { get;  }
        decimal Price { get; }
        DateTime TicksUTC { get;  }
    }

}
