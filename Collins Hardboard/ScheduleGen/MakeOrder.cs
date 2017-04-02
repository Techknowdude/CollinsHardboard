using System;
using System.Runtime.Remoting.Activation;
using System.Text;
using System.Threading.Tasks;
using ModelLib;

namespace ScheduleGen
{
    public class MakeOrder
    {
        private int _masterID;
        private int _piecesToMake;
        public DateTime DueDay { get; set; }

        public MakeOrder(int master, double pieces, DateTime dueDay = default(DateTime))
        {
            MasterID = master;
            PiecesToMake = (int)pieces;
            DueDay = dueDay;
        }

        public int MasterID
        {
            get
            {
                return _masterID;
            }

            set
            {
                _masterID = value;
            }
        }

        public int PiecesToMake
        {
            get
            {
                return _piecesToMake;
            }

            set
            {
                _piecesToMake = value;
            }
        }

        public static int DueDateComparerByDay(MakeOrder x, MakeOrder y)
        {
            if (x == null) return 1;
            if (y == null) return -1;

            return (y.DueDay - x.DueDay).Days;
        }
    }
}