using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EL.InfluxDB
{
    public interface IInfluxRecorder : IDisposable
    {
        void Record(params InfluxPoint[] points);
    }

    public class InfluxRecorder : IInfluxRecorder
    {
        readonly ISender sender;
        readonly IInfluxLogger logger;

        readonly InfluxSettings settings;
        private readonly ConcurrentQueue<string> queue;
        private readonly Timer timer;
        private bool isSending = false;

        public InfluxRecorder(ISender sender, InfluxSettings settings, IInfluxLogger logger)
        {
            this.sender = sender;
            this.settings = settings;
            this.logger = logger;

            queue = new ConcurrentQueue<string>();
            timer = new Timer(Send, null, TimeSpan.FromSeconds(settings.BatchIntervalInSeconds), TimeSpan.FromSeconds(settings.BatchIntervalInSeconds));
        }

        public void Dispose()
        {
            logger.Debug("Disposing...");
            timer.Dispose();
            Send(null);
        }

        public void Record(params InfluxPoint[] points)
        {
            foreach (var point in points)
            {
                queue.Enqueue(AssembleLineProtocol.Assemble(point));
            }
        }
        
        private void Send(object state)
        {
            if (isSending)
            {
                // NOTE: It's actually OK if two threads both send at the same time.
                // Nothing breaks, but it's a bit wasteful so we use a very simple check to avoid (not prevent) it.
                return;
            }
            isSending = true;
            
            var lastSentCount = settings.MaxBatchSize;
            while (!queue.IsEmpty && lastSentCount >= settings.MaxBatchSize)
            {
                lastSentCount = SendBatch(MakeBatch());
            }

            isSending = false;
        }

        private IList<string> MakeBatch()
        {
            var list = new List<string>();
            while (list.Count < settings.MaxBatchSize && !queue.IsEmpty)
            {
                if (queue.TryDequeue(out string line))
                {
                    list.Add(line);
                }
            }
            return list;
        }

        private int SendBatch(IList<string> points)
        {
            if (!points.Any())
            {
                return 0;
            }
            
            try
            {
                sender.SendPayload(string.Join("\n", points));
                return points.Count;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to send {points.Count} points to Influx", e);
            }

            return 0;
        }
    }
}
