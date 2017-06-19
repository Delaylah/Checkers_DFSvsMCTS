using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckersBoard
{
    public interface IListHelper
    {
        T Random<T>(IList<T> list);
    }

    public class ListHelpers : IListHelper
    {
        #region Dependencies

        private readonly IRandomService randomService;

        #endregion

        public ListHelpers(IRandomService randomService)
        {
            this.randomService = randomService;
        }

        public T Random<T>(IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            var count = list.Count();

            if (count == 0)
                return default(T);

            var randInt = randomService.Next(count);
            return list[randInt];
        }
    }
}
