using System.Collections.Concurrent;

namespace SimulationEngine
{
    public interface IPriceEngine
    {
        void Start();
        void Stop();
        bool IsStarted { get; }
        void Subscribe(string ticker);
        void Unsubscribe(string ticker);
        ConcurrentQueue<IMarketData> Feed { get; }
    }

}
