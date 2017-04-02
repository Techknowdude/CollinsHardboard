using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            PiecesToMake = (int) pieces;
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
    }
}
