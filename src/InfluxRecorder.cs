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

    internal class InfluxRecorder : IInfluxRecorder
    {
        private static readonly object timerLock = new object();
        private readonly IInfluxLogger logger;
        private readonly ConcurrentQueue<string> queue;
        private readonly ISender sender;
        private readonly IInfluxSettings settings;
        private bool isSending;
        private Timer timer;

        public InfluxRecorder(ISender sender, IInfluxLogger logger, IInfluxSettings settings)
        {
            this.sender = sender;
            this.settings = settings;
            this.logger = logger;

            queue = new ConcurrentQueue<string>();
        }

        public void Dispose()
        {
            logger.Debug("Disposing...");
            timer?.Dispose();
            Send(state: null);
        }

        public void Record(params InfluxPoint[] points)
        {
            foreach (var point in points)
            {
                queue.Enqueue(AssembleLineProtocol.Assemble(point));
            }

            StartTimerIfNotRunning();
        }

        private IList<string> MakeBatch()
        {
            var list = new List<string>();
            while (list.Count < settings.MaxBatchSize && !queue.IsEmpty)
            {
                if (queue.TryDequeue(out var line))
                {
                    list.Add(line);
                }
            }

            return list;
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

        private void StartTimerIfNotRunning()
        {
            if (timer != null) return;

            lock (timerLock)
            {
                if (settings.BatchIntervalInSeconds < 1)
                {
                    throw new InvalidOperationException("IInfluxSettings.BatchIntervalInSeconds must be no less than 1.");
                }
                timer = timer ?? (timer = new Timer(Send, state: null, TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(settings.BatchIntervalInSeconds)));
            }
        }
    }
}