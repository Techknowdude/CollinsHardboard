using System;
using System.Windows;

namespace ModelLib
{
    public class WiPItem : DependencyObject
    {
        public readonly DependencyProperty UnitProperty =
    DependencyProperty.Register("WiPItem", typeof(double),
    typeof(WiPItem), new UIPropertyMetadata(null));

        public readonly DependencyProperty UnitPiecesProperty =
    DependencyProperty.Register("WiPItem", typeof(double),
    typeof(WiPItem), new UIPropertyMetadata(null));

        public readonly DependencyProperty ProductCodeProperty =
            DependencyProperty.Register("WiPItem", typeof(string),
            typeof(WiPItem), new UIPropertyMetadata(null));

        public double Units
        {
            get { return (double)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public double PiecesPerUnit
        {
            get { return (double) GetValue(UnitPiecesProperty); }
            set { SetValue(UnitPiecesProperty, value); }
        }

        public String ProductCode
        {
            get { return (String) GetValue(ProductCodeProperty); }
            set { SetValue(ProductCodeProperty, value); }
        }

        public WiPItem(String code)
        {
            ProductCode = code;
        }

        public WiPItem(String code, double piecesPerUnit)
        {
            ProductCode = code;
            PiecesPerUnit = piecesPerUnit;
        }

        public WiPItem(String code, double units, double piecesPerUnit)
        {
            ProductCode = code;
            Units = units;
            PiecesPerUnit = piecesPerUnit;
        }
    }
}
