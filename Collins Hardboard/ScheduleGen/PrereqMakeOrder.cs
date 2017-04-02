using System.Collections.Generic;
using System.Linq;

namespace ScheduleGen
{
    public class PrereqMakeOrder : MakeOrder
    {
        /// <summary>
        /// Flag for if the prerequisites of this item have been scheduled.
        /// </summary>
        public bool AllPrereqScheduled
        {
            get { return PrereqOrders == null || !PrereqOrders.Any(); }
        }

        /// <summary>
        /// List of all things that need to be made before the item can be scheduled.
        /// </summary>
        public List<PrereqMakeOrder> PrereqOrders { get; set; } = new List<PrereqMakeOrder>();

        public PrereqMakeOrder(int master, double pieces) : base(master, pieces)
        {
            MasterID = master;
            PiecesToMake = (int)pieces;
        }

        /// <summary>
        /// Add an item that needs to be scheduled before this item can be ran
        /// </summary>
        /// <param name="order"></param>
        public void AddPrerequisite(PrereqMakeOrder order)
        {
            PrereqOrders.Add(order);
        }

        public void AddItemScheduled(int masterID, ref int pieces)
        {
            RemovePrereq(masterID, ref pieces);
        }

        /// <summary>
        /// Used after scheduling a prerequisite item.
        /// </summary>
        /// <param name="masterId"></param>
        /// <param name="pieces"></param>
        /// <returns></returns>
        private bool RemovePrereq(int masterId, ref int pieces)
        {
            bool removed = false;
            bool found = false;
            // check this level for matching item ID.
            var order = PrereqOrders.FirstOrDefault(p => p.MasterID == masterId);
            if(order != null)
            {
                removed = true;
                if (order.PiecesToMake < pieces)
                {
                    pieces -= order.PiecesToMake;
                    // remove the order 
                    if(order.AllPrereqScheduled)
                        PrereqOrders.Remove(order);
                }
                else
                {
                    order.PiecesToMake -= pieces;
                }
            }
            else
            {
                // if not found check each child
                foreach (var preOrder in PrereqOrders)
                {
                    removed = preOrder.RemovePrereq(masterId, ref pieces);
                    if (removed) break;
                }
            }

            return removed;
        }

    }
}