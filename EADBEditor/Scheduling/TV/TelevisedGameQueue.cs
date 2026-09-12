using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling
{
    public class TelevisedGameQueue
    {
        private Queue<TelevisedGame> queue;

        public bool IsEmpty => queue.Count == 0;

        private TelevisedGameQueue()
        {
        }

        public TelevisedGameQueue(IEnumerable<TelevisedGame> games)
        {
            this.queue = new Queue<TelevisedGame>(games);
        }

        /// <summary>
        /// dequeue until we empty the queue
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public bool TryDequeueGameForAssignment(out TelevisedGame game)
        {
            while (queue.Count > 0)
            {
                if (TryDequeueGame(out game))
                {
                    return true;
                }
            }

            game = null;
            return false;
        }

        private bool TryDequeueGame(out TelevisedGame game)
        {
            if (TryDequeue(out game) && !game.Assigned)
            {
                return true;
            }

            return false;
        }

        public TelevisedGame Dequeue() => queue.Dequeue();

        public IEnumerable<TelevisedGame> Dequeue(int take)
        {
            var list = new List<TelevisedGame>();

            for (int i = 0; i < take; i++)
            {
                if (!this.TryDequeue(out var game))
                {
                    break;
                }

                list.Add(game);
            }

            return list;
        }

        private bool TryDequeue(out TelevisedGame result)
        {
            if (queue.Count == 0)
            {
                result = null;
                return false;
            }

            result = queue.Dequeue();
            return true;
        }

        public void Requeue(TelevisedGame item)
        {
            var list = queue.ToArray();
            queue = new Queue<TelevisedGame>();
            queue.Enqueue(item);
            this.Enqueue(list);
        }

        public void Enqueue(IEnumerable<TelevisedGame> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }

        public void Enqueue(TelevisedGame game) => queue.Enqueue(game);
    }
}