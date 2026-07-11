using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EA_DB_Editor.Form1;

namespace EA_DB_Editor.Recruiting
{
    internal class TransferPortalClass
    {
        private const int BidLimit = 3;

        // durable state we keep for a portal offseason
        private static Dictionary<int, TransferPortalClass> transferPortalSignings = RecruitingFixup.PrestigeMap.ToDictionary(kvp => kvp.Key, kvp => new TransferPortalClass(kvp.Key));

        private bool[] positionsSigned = new bool[21];

        private int[] bidsSubmitted = new int[21];

        public TransferPortalClass(int teamId)
        {
            TeamId = teamId;
        }

        public int TeamId { get; }

        public void SignPlayer(int position)
        {
            positionsSigned[position] = true;
        }

        public bool OfferPlayer(int position)
        {
            // we already signed a player, don't bid for more
            if (positionsSigned[position])
            {
                return false;
            }

            if (bidsSubmitted[position] >= BidLimit)
            {
                return false;
            }

            bidsSubmitted[position]++;
            return true;
        }

        public void ResetBids()
        {
            bidsSubmitted = new int[21];
        }

        public static void SignPlayer(int teamId, int position)
        {
            transferPortalSignings[teamId].SignPlayer(position);
        }

        /// <summary>
        /// returns true if a bid was submitted, false otherwise
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool OfferPlayer(int teamId, TransferCandidate player)
        {
            return transferPortalSignings[teamId].OfferPlayer(player.PositionNumber);
        }

        public static void ResetTeamBids()
        {
            foreach (var team in transferPortalSignings.Values)
            {
                team.ResetBids();
            }
        }
    }
}