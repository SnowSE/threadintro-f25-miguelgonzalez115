namespace inclassLottery;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

class Ticket
{
    public int[] RegTickets { get; set; }
    public int PowerBall { get; set; }
    public Ticket(int[] numbers, int powerBall)
    {
        RegTickets = new int[5];
        for (int i = 0; i < 5; i++)
            RegTickets[i] = numbers[i];
        PowerBall = powerBall;
    }
    public Ticket()
    {
        RegTickets = new int[5];
        for (int i = 0; i < 5; i++)
        {
            RegTickets[i] = Random.Shared.Next(1, 70);
        }
        PowerBall = Random.Shared.Next(1, 27);
    }
    public int GetMatchLevel(Ticket winning)
    {
        int regularMatches = 0;
        foreach (int num in RegTickets)
        {
            if (winning.RegTickets.Contains(num))
                regularMatches++;
        }

        bool powerballMatch = (PowerBall == winning.PowerBall);

        if (regularMatches == 5 && powerballMatch) return 1; // power ball
        if (regularMatches == 5) return 2;
        if (regularMatches == 4 && powerballMatch) return 3;
        if (regularMatches == 4) return 4;
        if (regularMatches == 3 && powerballMatch) return 5;
        if (regularMatches == 3) return 6;
        if (regularMatches == 2 && powerballMatch) return 7;
        if (regularMatches == 1 && powerballMatch) return 8;
        if (powerballMatch) return 9;
        return 0; // if no win
    }
}
class LotteryPeriod
{
    public Ticket WinningTicket { get; set; }
    public List<Ticket> SoldTickets { get; set; } = new List<Ticket>();
    public LotteryPeriod()
    {
        int[] numbers = new int[5] { 1, 2, 3, 4, 5 };
        SetWinningTicket(numbers, 6);

    }
    public void SetWinningTicket(int[] numbers, int powerBall)
    {
        WinningTicket = new Ticket(numbers, powerBall);
    }
    public Dictionary<int, int> GatherStatistics()
    {
        Dictionary<int, int> stats = new Dictionary<int, int>();

        foreach (var ticket in SoldTickets)
        {
            int level = ticket.GetMatchLevel(WinningTicket);
            if (!stats.ContainsKey(level))
                stats[level] = 0;
            stats[level]++;
        }

        return stats;

    }
    public Dictionary<int, int> GatherStatisticsParallel()
    {
        var stats = new ConcurrentDictionary<int, int>();

        Parallel.ForEach(SoldTickets, ticket =>
        {
            int level = ticket.GetMatchLevel(WinningTicket);
            stats.AddOrUpdate(level, 1, (_, old) => old + 1);
        });

        return new Dictionary<int, int>(stats);
    }

}
class LotteryVendor
{
    public LotteryVendor()
    {
    }
    public void SellTickets(LotteryPeriod period, int numberOfTickets)
    {
        for (int i = 0; i < numberOfTickets; i++)
        {
            Ticket ticket = new Ticket();
            lock (period.SoldTickets)
            {
                period.SoldTickets.Add(ticket);
            }
        }
    }
}
class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, Lets sell 1Million Tickets!");
        LotteryPeriod period = new LotteryPeriod();
        //LotteryVendor vendor = new LotteryVendor();
        //vendor.SellTickets(period, 1_000_000);
        //Console.WriteLine("SOLD 1Million Tickets!");


        LotteryVendor v1 = new LotteryVendor();
        LotteryVendor v2 = new LotteryVendor();
        LotteryVendor v3 = new LotteryVendor();

        Thread t1 = new Thread(() => v1.SellTickets(period, 10_000_000));
        Thread t2 = new Thread(() => v2.SellTickets(period, 10_000_000));
        Thread t3 = new Thread(() => v3.SellTickets(period, 10_000_000));

        var startTime = DateTime.Now;

        t1.Start();
        t2.Start();
        t3.Start();

        t1.Join();
        t2.Join();
        t3.Join();

        var duration = DateTime.Now - startTime;

        Console.WriteLine($"Total Tickets Sold: {period.SoldTickets.Count}");

        
        Console.WriteLine($"Total Tickets Sold: {period.SoldTickets.Count:N0}");
        Console.WriteLine($"Time taken: {duration.TotalSeconds:F2} seconds\n");

        Console.WriteLine("Gathering statistics...");
        var stats = period.GatherStatisticsParallel(); // using parallel version

        Console.WriteLine("\n=== Lottery Results ===");
        foreach (var kv in stats.OrderBy(k => k.Key))
        {
            Console.WriteLine($"Level {kv.Key}: {kv.Value:N0} winners");
        }

        Console.WriteLine("\nSimulation complete!");

        //TODO: 1a) make 3 vendors sell 10M tickets each
        // 1b) 3 vendors sell tickets in parallel
        // 2) Modify Ticket class to be able to judge a winner level
        // 3) Gather statistics on how many winners of each level there are
        // 4) Print out the statistics
        // AFTER 1-4 is working, try to do (GatherStatistics) with Parallel Programming

    }
}
