using static NCAA14Utility.Scheduling.ScheduleBase;

namespace ScheduleMaker
{
    /// <summary>
    /// Games a season must have, set up before anything else by <see cref="SeasonInitializer"/> (as away, home).
    /// 2588 is a one-off: the independents play the return games of their 2587 schedules
    /// (D:\CFB27\export\independent_return_games.csv); from 2589 they're in conferences.
    /// </summary>
    internal static class RequiredGames
    {
        public static IReadOnlyList<(int Away, int Home)> For(int year) => year == 2588 ? Independents2588 : Array.Empty<(int, int)>();

        private static readonly (int Away, int Home)[] Independents2588 =
        {
            // at Delaware
            (SHSU, Delaware), (SacSt, Delaware), (NDSU, Delaware), (UMass, Delaware), (KennesawSt, Delaware), (FIU, Delaware),
            // at FIU
            (FAU, FIU), (MissouriSt, FIU), (JaxSt, FIU), (SacSt, FIU), (UMass, FIU), (KennesawSt, FIU),
            // at Jax State
            (Delaware, JaxSt), (NDSU, JaxSt), (KennesawSt, JaxSt), (SHSU, JaxSt), (Liberty, JaxSt),
            // at Kennesaw St.
            (NMSU, KennesawSt), (Liberty, KennesawSt), (UConn, KennesawSt), (NDSU, KennesawSt), (UMass, KennesawSt),
            // at Liberty
            (Delaware, Liberty), (FIU, Liberty), (UMass, Liberty), (MissouriSt, Liberty), (UConn, Liberty), (SacSt, Liberty),
            // at Massachusetts
            (JaxSt, UMass), (MissouriSt, UMass), (SacSt, UMass), (NDSU, UMass), (SHSU, UMass),
            // at Missouri State
            (Delaware, MissouriSt), (JaxSt, MissouriSt), (KennesawSt, MissouriSt), (NMSU, MissouriSt), (UConn, MissouriSt), (NDSU, MissouriSt),
            // at NDSU
            (FIU, NDSU), (Liberty, NDSU), (UConn, NDSU), (SacSt, NDSU), (NMSU, NDSU),
            // at New Mexico St.
            (Delaware, NMSU), (FIU, NMSU), (JaxSt, NMSU), (Liberty, NMSU), (UMass, NMSU), (SacSt, NMSU),
            // at Sac State
            (JaxSt, SacSt), (KennesawSt, SacSt), (MissouriSt, SacSt), (UConn, SacSt), (SHSU, SacSt),
            // at Sam Houston
            (KennesawSt, SHSU), (Liberty, SHSU), (MissouriSt, SHSU), (NDSU, SHSU), (NMSU, SHSU), (UConn, SHSU),
            // at UConn
            (Delaware, UConn), (FIU, UConn), (JaxSt, UConn), (UMass, UConn), (NMSU, UConn),
        };
    }
}
