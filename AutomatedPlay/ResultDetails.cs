using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedPlay
{
    public class ResultDetails
    {
        public MatchResult Result { get; set; }

        public TimeSpan Duration { get; set; }

        public Type WinningAlgorithmType { get; set; }


        public ResultDetails(MatchResult result, TimeSpan duration)
        {
            this.Result = result;
            this.Duration = duration;
        }

        public ResultDetails(MatchResult result, TimeSpan duration, Type winningAlgorithmType)
        {
            this.Result = result;
            this.Duration = duration;
            this.WinningAlgorithmType = winningAlgorithmType;
        }
    }
}
