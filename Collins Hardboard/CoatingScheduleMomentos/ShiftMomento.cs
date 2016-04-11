using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoatingScheduleMomentos
{
    public class ShiftMomento : ICoatingMomento
    {
        private List<ProductCaretakerBase> _caretakers;

        public List<ProductCaretakerBase> Caretakers
        {
            get { return _caretakers; }
            set { _caretakers = value; }
        }

        public ShiftMomento(List<ProductCaretakerBase> caretakers)
        {
            _caretakers = caretakers;
        }

        public ShiftMomento()
        {
            Caretakers = new List<ProductCaretakerBase>();
        }

        public ShiftMomento(ObservableCollection<ICoatingScheduleLogic> childrenLogic)
        {
            foreach (ICoatingScheduleLogic logic in ChildrenLogic)
            {
                CoatingScheduleProduct product = logic as CoatingScheduleProduct;
                CoatingScheduleNote note = logic as CoatingScheduleNote;
                ICoatingMomento newMomento = null;

                if (product != null)
                {
                    newMomento = product.CreateMomento();
                }
                else if (note != null)
                {
                    newMomento = note.CreateMomento();
                }
                newCaretakerBase = ProductCaretakerBase.CreateProductCaretaker(newMomento);

                shiftMomento.Caretakers.Add(newCaretakerBase);
            }
        }
    }
}
