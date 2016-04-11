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

        public MakeOrder(int master, int pieces)
        {
            MasterID = master;
            PiecesToMake = pieces;
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
