using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckersBoard;
using System.Diagnostics;

namespace AutomatedPlay
{
    class Program
    {
        private static int MATCHES_COUNT = 50 ;
        private static int MAX_TURNS_PER_MATCH = 200;

        private static IList<IAIAlgorithm> algorithms;

        private static Dictionary<MatchResult, int> resultsByColor = new Dictionary<MatchResult, int>();
        private static Dictionary<Type, int> resultsByAlgorithm = new Dictionary<Type, int>();


        static void Main(string[] args)
        {
            var randomService = new RandomService();
            var listHelper = new ListHelpers(randomService);

            algorithms = new List<IAIAlgorithm>
            {
                //new AgresiveRandomAI(),
                new DfsAI(listHelper),
                new MctsAI(listHelper, randomService)
            };



            for (var i = 0; i < MATCHES_COUNT; i++)
            {
                var shuffeledAlgorithms = algorithms.OrderBy(o => Guid.NewGuid()).ToArray();
                var algorithmsByPlayers = new Dictionary<Player, IAIAlgorithm>
                {
                    { Player.Black, shuffeledAlgorithms[0] },
                    { Player.Red, shuffeledAlgorithms[1] }
                };

                var resultDetails = PlayMatch(algorithmsByPlayers);
                Console.WriteLine($"Match #{i + 1}, {resultDetails.Result}, {resultDetails.Duration}; {resultDetails.WinningAlgorithmType?.Name}");

                // Results by color
                if (!resultsByColor.ContainsKey(resultDetails.Result)) resultsByColor[resultDetails.Result] = 0;
                resultsByColor[resultDetails.Result]++;

                // Results by algorithm
                Player? rp = null;
                if (resultDetails.Result == MatchResult.BlackWon)
                    rp = Player.Black;
                else if (resultDetails.Result == MatchResult.RedWon)
                    rp = Player.Red;

                if (rp.HasValue)
                {
                    var a = algorithmsByPlayers[rp.Value];
                    if (!resultsByAlgorithm.ContainsKey(a.GetType()))
                        resultsByAlgorithm[a.GetType()] = 0;

                    resultsByAlgorithm[a.GetType()]++;
                }
            }

            Console.WriteLine("Results by color:");
            foreach (var kv in resultsByColor)
                Console.WriteLine($"\t{kv.Key}: {kv.Value}");

            Console.WriteLine("Results by algorithm:");
            foreach (var kv in resultsByAlgorithm)
                Console.WriteLine($"\t{kv.Key.Name}: {kv.Value}");

            Console.ReadLine();
        }

        private static ResultDetails PlayMatch(Dictionary<Player, IAIAlgorithm> algorithms)
        {
            var board = new CheckerBoard();
            board.InitializeBoard();
            
            Stopwatch matchStopwatch = new Stopwatch();
            Stopwatch stopwatch = new Stopwatch();
            int turns = 0;

            matchStopwatch.Start();
            while (board.GetGameStatus() == GameStatuses.Running)
            {
                stopwatch.Reset();
                stopwatch.Start();
                var move = algorithms[board.NextPlayer].GetMove(board, board.NextPlayer);
                stopwatch.Stop();

                if (move == null)
                    return new ResultDetails(MatchResult.NoMoreMoves, matchStopwatch.Elapsed);

                Console.WriteLine($"\tTurn #{turns}; Player {board.NextPlayer}; ({move.piece1.Row}, {move.piece1.Column}) to ({move.piece2.Row}, {move.piece2.Column}); Elapsed: {stopwatch.ElapsedMilliseconds} ms");

                board.MakeMove(move, board.NextPlayer);

                if (board.GetGameStatus() != GameStatuses.Running)
                {
                    var result = board.GetGameStatus();

                    if (result == GameStatuses.RedWon)
                        return new ResultDetails(MatchResult.RedWon, matchStopwatch.Elapsed, algorithms[board.NextPlayer].GetType());
                    else if (result == GameStatuses.BlackWon)
                        return new ResultDetails(MatchResult.BlackWon, matchStopwatch.Elapsed, algorithms[board.NextPlayer].GetType());
                    else
                        throw new Exception($"Unsupported result: {result}");
                }

                if (++turns >= MAX_TURNS_PER_MATCH)
                    return new ResultDetails(MatchResult.MaxTurnsReached, matchStopwatch.Elapsed);
            }

            return new ResultDetails(MatchResult.NoResult, matchStopwatch.Elapsed);
        }
    }
}
