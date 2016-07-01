using System;
using CoatingScheduler.Annotations;
using ModelLib;
using StaticHelpers;

namespace ProductionScheduler
{
    [Serializable]
    public class PressMasterItem : ObservableObject
    {
        private ProductMasterItem _masterItem;
        private double _unitsMade;
        private double _thickness;
        private string _name;
        private string _thicknessString;

        public double UnitsMade
        {
            get
            {
                return _unitsMade;
            }

            set
            {
                _unitsMade = value;
                RaisePropertyChangedEvent();
            }
        }

        public double Thickness
        {
            get { return _thickness; }
            set
            {
                _thickness = value;
                ThicknessString = StaticFunctions.ConvertDoubleToStringThickness(value);
                RaisePropertyChangedEvent();
            }
        }

        public String ThicknessString
        {
            get { return _thicknessString; }
            set
            {
                _thicknessString = value;
                RaisePropertyChangedEvent();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChangedEvent();
            }
        }

        public ProductMasterItem MasterItem
        {
            get
            {
                return _masterItem;
            }

            set
            {
                _masterItem = value;
                RaisePropertyChangedEvent();
            }
        }

        public PressMasterItem([NotNull]ProductMasterItem item, double units)
        {
            MasterItem = item;
            UnitsMade = units;
            Thickness = item.Thickness;
            Name = item.Description;
        }

        public PressMasterItem()
        {
            UnitsMade = 0;
            Thickness = .5;
            Name = "Description";
        }
    }
}
