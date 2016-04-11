using System;
using System.Windows.Controls;
using StaticHelpers;

namespace Main_Application
{
    /// <summary>
    /// Interaction logic for GradeControl.xaml
    /// </summary>
    public partial class GradeControl : UserControl
    {
        #region Fields

        private string _gradeAbbr;
        private string _gradeName;

        #endregion

        #region Properties

        public String GradeName
        {
            get { return _gradeName; }
            set
            {
                StaticFactoryValuesManager.GradesList[Index] = value;
                _gradeName = value;
            }
        }

        public String GradeAbbr
        {
            get { return _gradeAbbr; }
            set
            {
                StaticFactoryValuesManager.GradeAbbrList[Index] = value;
                _gradeAbbr = value;
            }
        }


        public int Index { get; set; }

        #endregion

        public GradeControl(String gradeName, String gradeAbbr, int index)
        {
            InitializeComponent();
            DataContext = this;

            Index = index;

            _gradeName = gradeName;
            _gradeAbbr = gradeAbbr;
        }
    }
}
