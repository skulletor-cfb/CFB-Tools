using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EA_DB_Editor.Scheduling
{
    public static class ConferenceGameSlateHelper
    {
        private static readonly List<ConferenceGameSlate> conferences = new List<ConferenceGameSlate>
            {
            new Pac12(),
            new Big10(),
            new Big12(),
            new ACC(),
            new SEC(),
            new CUSA(),
            new MWC(),
            new MAC(),
            new SunBelt(),
            new American(),
            new NotreDame(),
        };

        private static readonly Dictionary<int, ConferenceGameSlate> conferenceDict = conferences.ToDictionary(c => c.Id);

        public static Dictionary<int, ConferenceGameSlate> Organize(this TelevisedGame[] games)
        {
            foreach (var game in games)
            {
                conferenceDict[game.ConferenceOwner].Games.Add(game);
            }

            return conferenceDict;
        }

        public static void MakeOffers()
        {
            conferences.ForEach(c => c.MakeOffers());

            if (conferences.SelectMany(c => c.Games).Where(g => !g.Selected).Any())
            {
                MessageBox.Show("Not all games selected");
            }
        }


        public static void DistributeUnassigned()
        {
            conferences.ForEach(c => c.DistributeUnassigned());
        }
    }

    public abstract class ConferenceGameSlate
    {
        public List<TelevisedGame> Games { get; }

        protected IEnumerable<TelevisedGame> UnassignedGames => Games.Where(g => !g.Assigned).Select(g => g.Deselect());

        public int Id { get; }
        protected ConferenceGameSlate(int conferenceId)
        {
            Id = conferenceId;
            Games = new List<TelevisedGame>();
        }

        public abstract void MakeOffers();

        public abstract void DistributeUnassigned();
    }
    public class NotreDame : ConferenceGameSlate
    {
        public NotreDame() : base(TableUtility.NotreDameId)
        {
        }

        public override void DistributeUnassigned()
        {
        }

        public override void MakeOffers()
        {
            NBCNetwork.Instance.Offer(Games);
        }
    }
    public class Pac12 : ConferenceGameSlate
    {
        public Pac12() : base(TableUtility.Pac16Id)
        {
        }

        public override void DistributeUnassigned()
        {
            var queue = UnassignedGames.ToQueue();
            while (!queue.IsEmpty)
            {
                FoxNetworks.Instance.Offer(queue.Dequeue(1));
                ESPNNetworks.Instance.Offer(queue.Dequeue(1));
            }
        }

        public override void MakeOffers()
        {
            // fox, espn, cbs all get pac 12 content
            var games = Games.GetAvailableGamesByWeek();
            for (int i = 0; i <= 13; i++)
            {
                var queue = games[i].ToQueue();
                ESPNNetworks.Instance.Offer(queue.Dequeue(1));
                FoxNetworks.Instance.Offer(queue.Dequeue(1));
            }

            // fox and espn split the rest
            var remainingGameQueue = Games.Where(g => !g.Selected).OrderBy(g => g.Week).ThenBy(g => g.Score).ToQueue();
            while (!remainingGameQueue.IsEmpty)
            {
                FoxNetworks.Instance.Offer(remainingGameQueue.Dequeue(1));
                ESPNNetworks.Instance.Offer(remainingGameQueue.Dequeue(1));
            }
        }
    }

    public class Big10 : ConferenceGameSlate
    {
        public Big10() : base(TableUtility.Big10Id)
        {
        }

        public override void DistributeUnassigned()
        {
            FoxNetworks.Instance.Offer(UnassignedGames);
        }

        public override void MakeOffers()
        {
            // NBC gets the 4 conference and non conference worst games for peacock
            Peacock.Instance.Offer(Games.Where(g => g.IsConferenceGame && g.Week == 0).OrderByDescending(g => g.Score).Draft(1));
            Peacock.Instance.Offer(Games.Where(g => g.IsConferenceGame && g.Week > 0).OrderByDescending(g => g.Score).Draft(3));
            Peacock.Instance.Offer(Games.Where(g => !g.IsConferenceGame).OrderByDescending(g => g.Score).Draft(4));

            // the rest go to fox
            FoxNetworks.Instance.Offer(Games);
        }
    }

    public class Big12 : ConferenceGameSlate
    {
        public Big12() : base(TableUtility.Big12Id)
        {
        }

        public override void DistributeUnassigned()
        {
            var queue = UnassignedGames.ToQueue();
            while (!queue.IsEmpty)
            {
                FoxNetworks.Instance.Offer(queue.Dequeue(1));
                ESPNNetworks.Instance.Offer(queue.Dequeue(1));
            }
        }

        public override void MakeOffers()
        {
            // ESPN gets the first 3 because it already got RRS
            ESPNNetworks.Instance.Offer(Games.Draft(3));

            // fox gets the last 4
            FoxNetworks.Instance.Offer(Games.OrderByDescending(g => g.Score).Draft(4));

            // rest of the games are split evenly
            var games = Games.GetAvailableGamesByWeek();
            var topWeek13GameTaken = false;
            foreach (var kvp in games)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        ESPNNetworks.Instance.Offer(kvp.Value[i]);
                    }
                    else
                    {
                        if (kvp.Key == 13 && !topWeek13GameTaken)
                        {
                            // last week of the season, CBS take's FOX big 12
                            CBSNetwork.Instance.Offer(kvp.Value[i]);
                            topWeek13GameTaken = true;
                        }
                        else
                        {
                            FoxNetworks.Instance.Offer(kvp.Value[i]);
                        }
                    }
                }
            }
        }
    }

    public class ACC : ConferenceGameSlate
    {
        public ACC() : base(TableUtility.ACCId)
        {
        }

        public override void DistributeUnassigned()
        {
            ESPNNetworks.Instance.Offer(UnassignedGames);
        }

        public override void MakeOffers()
        {
            // take the unselected acc games, 1 per week the weakest non fcs game of the week
            var accGames = Games.GetAvailableGamesByWeek(orderFunc: g => -g.Score);
            foreach (var kvp in accGames)
            {
                var cwAssigned = false;
                var queue = kvp.Value.ToQueue();

                while (queue.TryDequeueGameForAssignment(out var game))
                {
                    if (game.IsFCSGame || cwAssigned)
                    {
                        ESPNNetworks.Instance.Offer(game);
                    }
                    else
                    {
                        CWNetwork.Instance.Offer(game);
                        cwAssigned = true;
                    }
                }
            }
        }
    }

    public class SEC : ConferenceGameSlate
    {
        public SEC() : base(TableUtility.SECId)
        {
        }

        public override void DistributeUnassigned()
        {
            ESPNNetworks.Instance.Offer(UnassignedGames);
        }
        public override void MakeOffers()
        {
            ESPNNetworks.Instance.Offer(Games);
        }
    }

    public class SunBelt : ConferenceGameSlate
    {
        public SunBelt() : base(TableUtility.SBCId)
        {
        }

        public override void DistributeUnassigned()
        {
            ESPNNetworks.Instance.Offer(UnassignedGames);
        }
        public override void MakeOffers()
        {
            ESPNNetworks.Instance.Offer(Games);
        }
    }

    public class American : ConferenceGameSlate
    {
        public American() : base(TableUtility.AmericanId)
        {
        }

        public override void DistributeUnassigned()
        {
            ESPNNetworks.Instance.Offer(UnassignedGames);
        }
        public override void MakeOffers()
        {
            ESPNNetworks.Instance.Offer(Games);
        }
    }

    public class CUSA : ConferenceGameSlate
    {
        public CUSA() : base(TableUtility.CUSAId)
        {
        }

        public override void DistributeUnassigned()
        {
            ESPNNetworks.Instance.Offer(UnassignedGames);
        }
        public override void MakeOffers()
        {
            // CBSSN gets 1 game a week
            var games = Games.GetAvailableGamesByWeek();
            for (int i = 0; i <= 13; i++)
            {
                var queue = games[i].ToQueue();
                CBSSportsNetwork.Instance.Offer(queue.Dequeue(1));
            }

            // the rest go to espn
            ESPNNetworks.Instance.Offer(Games);
        }
    }

    public class MAC : ConferenceGameSlate
    {
        public MAC() : base(TableUtility.MACId)
        {
        }

        public override void DistributeUnassigned()
        {
            ESPNNetworks.Instance.Offer(UnassignedGames);
        }
        public override void MakeOffers()
        {
            // each week CBSSN, starting with maction it gets 2.  rest go to ESPN
            var games = Games.GetAvailableGamesByWeek();
            for (int i = 0; i <= 12; i++)
            {
                if (!games.ContainsKey(i))
                {
                    continue;
                }

                var queue = games[i].ToQueue();
                ESPNNetworks.Instance.Offer(queue.Dequeue(1));
                CBSSportsNetwork.Instance.Offer(queue.Dequeue(1));

                if (i >= TelevisionScheduler.MACtionStartWeek)
                {
                    CBSSportsNetwork.Instance.Offer(queue.Dequeue(1));
                }
            }

            var w13 = games[13];

            // for week 13, both espn and cbs get 3 games
            for (int i = 0; i < w13.Count; i++)
            {
                if (i % 2 == 0)
                {
                    CBSSportsNetwork.Instance.Offer(w13[i]);
                }
                else
                {
                    ESPNNetworks.Instance.Offer(w13[i]);
                }
            }

            // the rest go to espn
            ESPNNetworks.Instance.Offer(Games);
        }
    }

    public class MWC : ConferenceGameSlate
    {
        public MWC() : base(TableUtility.MWCId)
        {
        }
        public override void DistributeUnassigned()
        {
            MountainWestSportsNetwork.Instance.Offer(UnassignedGames);
        }

        public override void MakeOffers()
        {
            // cbs gets the best of the year
            CBSNetwork.Instance.Offer(Games.Choose(1));

            // cw gets 2 one for friday/saturday
            var games = Games.GetAvailableGamesByWeek();
            for (int i = 0; i <= 13; i++)
            {
                var queue = games[i].ToQueue();
                CWNetwork.Instance.Offer(queue.Dequeue(1));
                CBSSportsNetwork.Instance.Offer(queue.Dequeue(1));
                FoxNetworks.Instance.Offer(queue.Dequeue(1));
                CWNetwork.Instance.Offer(queue.Dequeue(1));
                CBSSportsNetwork.Instance.Offer(queue.Dequeue(1));
                CWNetwork.Instance.Offer(queue.Dequeue(1));
            }

            // the rest go to the mountain for streaming
            MountainWestSportsNetwork.Instance.Offer(Games);
        }
    }
}