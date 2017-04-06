using System.Collections.Generic;
using System.Linq;
using Configuration_windows;
using ImportLib;
using ModelLib;

namespace ScheduleGen
{
    public class PrereqMakeOrder : MakeOrder
    {
        public int Priority { get; set; }
        public bool AllPrereqScheduled
        {
            get { return PrereqOrders == null || !PrereqOrders.Any(); }
        }

        /// <summary>
        /// List of all things that need to be made before the item can be scheduled.
        /// </summary>
        public List<PrereqMakeOrder> PrereqOrders { get; set; } = new List<PrereqMakeOrder>();

        public PrereqMakeOrder(int master, double pieces, int priority = 0) : base(master, pieces)
        {
            MasterID = master;
            PiecesToMake = (int)pieces;
            Priority = priority;
        }

        /// <summary>
        /// Add an item that needs to be scheduled before this item can be ran
        /// </summary>
        /// <param name="order"></param>
        public void AddPrerequisite(PrereqMakeOrder order)
        {
            order.Priority = Priority;
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

        private PrereqMakeOrder GetLowestRequirement(ref int deepestDepth, int depth, string plant, string line)
        {
            var master = StaticInventoryTracker.ProductMasterList.FirstOrDefault(m => m.MasterID == MasterID);
            int thisDepth = depth + 1;

            // check children
            if (PrereqOrders.Any())
            {
                PrereqMakeOrder deepestOrder = null;
                foreach (var prereqMakeOrder in PrereqOrders)
                {
                    var nextOrder = prereqMakeOrder.GetLowestRequirement(ref deepestDepth, thisDepth, plant, line);
                    if (nextOrder != null && thisDepth > deepestDepth)
                    {
                        deepestOrder = nextOrder;
                        deepestDepth = thisDepth;
                    }
                }
                if(deepestOrder != null)
                    return deepestOrder;

            }

            // true if no line specified, 
            bool runOnLine = string.IsNullOrEmpty(line) || CanMakeOnLine(master, line);
            // check for items that are made in that plant
            if ((string.IsNullOrEmpty(plant) || master.MadeIn.ToUpper().Equals(plant.ToUpper())) && runOnLine)
            {
                return this;
            }

            return null;
        }

        private bool CanMakeOnLine(ProductMasterItem master, string line)
        {
            // check all machines that can run on that line
            List<Machine> machines = MachineHandler.Instance.MachineList.Where(m => m.LinesCanRunOn.Contains(line)).ToList();
            if (machines.Any())
            {
                return machines.Any(m => m.ConfigurationList.Any(c => c.CanMake(master)));
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the lowest requirement and returns the order for it.
        /// </summary>
        /// <param name="plant">COATING or PRESS is valid</param>
        /// <returns></returns>
        public PrereqMakeOrder GetLowestRequirement(string plant = null, string line = null)
        {
            int depth = 0;
            return GetLowestRequirement(ref depth, 0, plant, line);
        }

        public bool RemoveReq(PrereqMakeOrder pre)
        {
            if (!PrereqOrders.Remove(pre))
            {
                foreach (var prereqMakeOrder in PrereqOrders)
                {
                    if(prereqMakeOrder.RemoveReq(pre))
                        return true;
                }
            }
            else
            {
                return true;
            }

            return false;
        }
    }
}