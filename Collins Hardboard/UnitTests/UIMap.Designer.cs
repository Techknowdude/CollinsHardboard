﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by coded UI test builder.
//      Version: 12.0.0.0
//
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public partial class UIMap
    {
        
        /// <summary>
        /// Open schedule and create new product
        /// </summary>
        public void OpenCoatingSchedule()
        {
            #region Variable Declarations
            WpfTitleBar uICoatingScheduleTitleBar = this.UICoatingScheduleWindow.UICoatingScheduleTitleBar;
            WpfButton uIAddDayButton = this.UICoatingScheduleWindow.UIAddDayButton;
            WpfButton uIAddShiftButton = this.UICoatingScheduleWindow.UISchedulerListViewList.UIMain_ApplicationDayCListItem.UIAddShiftButton;
            WpfButton uIAddProductButton = this.UICoatingScheduleWindow.UIItemCustom.UIShiftListViewList.UIMain_ApplicationShifListItem.UIAddProductButton;
            WpfEdit uITxbThicknessEdit = this.UICoatingScheduleWindow.UIItemCustom1.UIShiftListViewList.UIMain_ApplicationProdListItem.UIThisCustom.UITxbThicknessEdit;
            #endregion

            // Launch '%USERPROFILE%\Documents\collins-hardboard-scheduler\Collins Hardboard\Main Application\bin\Debug\Main Application.exe'
            ApplicationUnderTest mainApplicationApplication = ApplicationUnderTest.Launch(this.OpenCoatingScheduleParams.ExePath, this.OpenCoatingScheduleParams.AlternateExePath);

            // Click 'CoatingSchedule' title bar
            Mouse.Click(uICoatingScheduleTitleBar, new Point(289, 11));

            // Click 'Add Day' button
            Mouse.Click(uIAddDayButton, new Point(65, 19));

            // Click 'Add Shift' button
            Mouse.Click(uIAddShiftButton, new Point(52, 12));

            // Click 'Add Product' button
            Mouse.Click(uIAddProductButton, new Point(62, 10));

            // Click 'TxbThickness' text box
            Mouse.Click(uITxbThicknessEdit, new Point(44, 16));
        }
        
        /// <summary>
        /// ThicknessCheckedTest - Use 'ThicknessCheckedTestExpectedValues' to pass parameters into this method.
        /// </summary>
        public void ThicknessCheckedTest()
        {
            #region Variable Declarations
            WpfEdit uITxbThicknessEdit = this.UICoatingScheduleWindow.UIItemCustom1.UIShiftListViewList.UIMain_ApplicationProdListItem.UIThisCustom.UITxbThicknessEdit;
            #endregion

            // Verify that the 'Text' property of 'TxbThickness' text box equals 'TestingThick'
            Assert.AreEqual(this.ThicknessCheckedTestExpectedValues.UITxbThicknessEditText, uITxbThicknessEdit.Text, "Thickness not loaded");
        }
        
        /// <summary>
        /// CloseWindow
        /// </summary>
        public void CloseWindow()
        {
            #region Variable Declarations
            WpfButton uICloseButton = this.UICoatingScheduleWindow.UICoatingScheduleTitleBar.UICloseButton;
            WpfButton uICloseButton1 = this.UIHardboardSchedulerWindow.UIHardboardSchedulerTitleBar.UICloseButton;
            #endregion

            // Click 'Close' button
            Mouse.Click(uICloseButton, new Point(30, 4));

            // Click 'Close' button
            Mouse.Click(uICloseButton1, new Point(19, 19));
        }
        
        #region Properties
        public virtual OpenCoatingScheduleParams OpenCoatingScheduleParams
        {
            get
            {
                if ((this.mOpenCoatingScheduleParams == null))
                {
                    this.mOpenCoatingScheduleParams = new OpenCoatingScheduleParams();
                }
                return this.mOpenCoatingScheduleParams;
            }
        }
        
        public virtual ThicknessCheckedTestExpectedValues ThicknessCheckedTestExpectedValues
        {
            get
            {
                if ((this.mThicknessCheckedTestExpectedValues == null))
                {
                    this.mThicknessCheckedTestExpectedValues = new ThicknessCheckedTestExpectedValues();
                }
                return this.mThicknessCheckedTestExpectedValues;
            }
        }
        
        public UICoatingScheduleWindow UICoatingScheduleWindow
        {
            get
            {
                if ((this.mUICoatingScheduleWindow == null))
                {
                    this.mUICoatingScheduleWindow = new UICoatingScheduleWindow();
                }
                return this.mUICoatingScheduleWindow;
            }
        }
        
        public UIHardboardSchedulerWindow UIHardboardSchedulerWindow
        {
            get
            {
                if ((this.mUIHardboardSchedulerWindow == null))
                {
                    this.mUIHardboardSchedulerWindow = new UIHardboardSchedulerWindow();
                }
                return this.mUIHardboardSchedulerWindow;
            }
        }
        #endregion
        
        #region Fields
        private OpenCoatingScheduleParams mOpenCoatingScheduleParams;
        
        private ThicknessCheckedTestExpectedValues mThicknessCheckedTestExpectedValues;
        
        private UICoatingScheduleWindow mUICoatingScheduleWindow;
        
        private UIHardboardSchedulerWindow mUIHardboardSchedulerWindow;
        #endregion
    }
    
    /// <summary>
    /// Parameters to be passed into 'OpenCoatingSchedule'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class OpenCoatingScheduleParams
    {
        
        #region Fields
        /// <summary>
        /// Launch '%USERPROFILE%\Documents\collins-hardboard-scheduler\Collins Hardboard\Main Application\bin\Debug\Main Application.exe'
        /// </summary>
        public string ExePath = "C:\\Users\\Brandon\\Documents\\collins-hardboard-scheduler\\Collins Hardboard\\Main App" +
            "lication\\bin\\Debug\\Main Application.exe";
        
        /// <summary>
        /// Launch '%USERPROFILE%\Documents\collins-hardboard-scheduler\Collins Hardboard\Main Application\bin\Debug\Main Application.exe'
        /// </summary>
        public string AlternateExePath = "%USERPROFILE%\\Documents\\collins-hardboard-scheduler\\Collins Hardboard\\Main Applic" +
            "ation\\bin\\Debug\\Main Application.exe";
        #endregion
    }
    
    /// <summary>
    /// Parameters to be passed into 'ThicknessCheckedTest'
    /// </summary>
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class ThicknessCheckedTestExpectedValues
    {
        
        #region Fields
        /// <summary>
        /// Verify that the 'Text' property of 'TxbThickness' text box equals 'TestingThick'
        /// </summary>
        public string UITxbThicknessEditText = "TestingThick";
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UICoatingScheduleWindow : WpfWindow
    {
        
        public UICoatingScheduleWindow()
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.Name] = "CoatingSchedule";
            this.SearchProperties.Add(new PropertyExpression(PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public UICoatingScheduleTitleBar UICoatingScheduleTitleBar
        {
            get
            {
                if ((this.mUICoatingScheduleTitleBar == null))
                {
                    this.mUICoatingScheduleTitleBar = new UICoatingScheduleTitleBar(this);
                }
                return this.mUICoatingScheduleTitleBar;
            }
        }
        
        public WpfButton UIAddDayButton
        {
            get
            {
                if ((this.mUIAddDayButton == null))
                {
                    this.mUIAddDayButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUIAddDayButton.SearchProperties[WpfButton.PropertyNames.AutomationId] = "BtnAddDay";
                    this.mUIAddDayButton.WindowTitles.Add("CoatingSchedule");
                    #endregion
                }
                return this.mUIAddDayButton;
            }
        }
        
        public UISchedulerListViewList UISchedulerListViewList
        {
            get
            {
                if ((this.mUISchedulerListViewList == null))
                {
                    this.mUISchedulerListViewList = new UISchedulerListViewList(this);
                }
                return this.mUISchedulerListViewList;
            }
        }
        
        public UIItemCustom UIItemCustom
        {
            get
            {
                if ((this.mUIItemCustom == null))
                {
                    this.mUIItemCustom = new UIItemCustom(this);
                }
                return this.mUIItemCustom;
            }
        }
        
        public UIItemCustom1 UIItemCustom1
        {
            get
            {
                if ((this.mUIItemCustom1 == null))
                {
                    this.mUIItemCustom1 = new UIItemCustom1(this);
                }
                return this.mUIItemCustom1;
            }
        }
        #endregion
        
        #region Fields
        private UICoatingScheduleTitleBar mUICoatingScheduleTitleBar;
        
        private WpfButton mUIAddDayButton;
        
        private UISchedulerListViewList mUISchedulerListViewList;
        
        private UIItemCustom mUIItemCustom;
        
        private UIItemCustom1 mUIItemCustom1;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UICoatingScheduleTitleBar : WpfTitleBar
    {
        
        public UICoatingScheduleTitleBar(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.AutomationId] = "TitleBar";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public WpfButton UICloseButton
        {
            get
            {
                if ((this.mUICloseButton == null))
                {
                    this.mUICloseButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUICloseButton.SearchProperties[WpfButton.PropertyNames.Name] = "Close";
                    this.mUICloseButton.WindowTitles.Add("CoatingSchedule");
                    #endregion
                }
                return this.mUICloseButton;
            }
        }
        #endregion
        
        #region Fields
        private WpfButton mUICloseButton;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UISchedulerListViewList : WpfList
    {
        
        public UISchedulerListViewList(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.AutomationId] = "SchedulerListView";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public UIMain_ApplicationDayCListItem UIMain_ApplicationDayCListItem
        {
            get
            {
                if ((this.mUIMain_ApplicationDayCListItem == null))
                {
                    this.mUIMain_ApplicationDayCListItem = new UIMain_ApplicationDayCListItem(this);
                }
                return this.mUIMain_ApplicationDayCListItem;
            }
        }
        #endregion
        
        #region Fields
        private UIMain_ApplicationDayCListItem mUIMain_ApplicationDayCListItem;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIMain_ApplicationDayCListItem : WpfListItem
    {
        
        public UIMain_ApplicationDayCListItem(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.Name] = "Main_Application.DayControl";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public WpfButton UIAddShiftButton
        {
            get
            {
                if ((this.mUIAddShiftButton == null))
                {
                    this.mUIAddShiftButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUIAddShiftButton.SearchProperties[WpfButton.PropertyNames.AutomationId] = "BtnAddLine";
                    this.mUIAddShiftButton.WindowTitles.Add("CoatingSchedule");
                    #endregion
                }
                return this.mUIAddShiftButton;
            }
        }
        #endregion
        
        #region Fields
        private WpfButton mUIAddShiftButton;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIItemCustom : WpfCustom
    {
        
        public UIItemCustom(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.ClassName] = "Uia.LineControl";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public UIShiftListViewList UIShiftListViewList
        {
            get
            {
                if ((this.mUIShiftListViewList == null))
                {
                    this.mUIShiftListViewList = new UIShiftListViewList(this);
                }
                return this.mUIShiftListViewList;
            }
        }
        #endregion
        
        #region Fields
        private UIShiftListViewList mUIShiftListViewList;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIShiftListViewList : WpfList
    {
        
        public UIShiftListViewList(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.AutomationId] = "ShiftListView";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public UIMain_ApplicationShifListItem UIMain_ApplicationShifListItem
        {
            get
            {
                if ((this.mUIMain_ApplicationShifListItem == null))
                {
                    this.mUIMain_ApplicationShifListItem = new UIMain_ApplicationShifListItem(this);
                }
                return this.mUIMain_ApplicationShifListItem;
            }
        }
        #endregion
        
        #region Fields
        private UIMain_ApplicationShifListItem mUIMain_ApplicationShifListItem;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIMain_ApplicationShifListItem : WpfListItem
    {
        
        public UIMain_ApplicationShifListItem(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.Name] = "Main_Application.ShiftControl";
            this.SearchProperties[PropertyNames.Instance] = "2";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public WpfButton UIAddProductButton
        {
            get
            {
                if ((this.mUIAddProductButton == null))
                {
                    this.mUIAddProductButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUIAddProductButton.SearchProperties[WpfButton.PropertyNames.AutomationId] = "BtnAddProduct";
                    this.mUIAddProductButton.WindowTitles.Add("CoatingSchedule");
                    #endregion
                }
                return this.mUIAddProductButton;
            }
        }
        #endregion
        
        #region Fields
        private WpfButton mUIAddProductButton;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIItemCustom1 : WpfCustom
    {
        
        public UIItemCustom1(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.ClassName] = "Uia.ShiftControl";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public UIShiftListViewList1 UIShiftListViewList
        {
            get
            {
                if ((this.mUIShiftListViewList == null))
                {
                    this.mUIShiftListViewList = new UIShiftListViewList1(this);
                }
                return this.mUIShiftListViewList;
            }
        }
        #endregion
        
        #region Fields
        private UIShiftListViewList1 mUIShiftListViewList;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIShiftListViewList1 : WpfList
    {
        
        public UIShiftListViewList1(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.AutomationId] = "ShiftListView";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public UIMain_ApplicationProdListItem UIMain_ApplicationProdListItem
        {
            get
            {
                if ((this.mUIMain_ApplicationProdListItem == null))
                {
                    this.mUIMain_ApplicationProdListItem = new UIMain_ApplicationProdListItem(this);
                }
                return this.mUIMain_ApplicationProdListItem;
            }
        }
        #endregion
        
        #region Fields
        private UIMain_ApplicationProdListItem mUIMain_ApplicationProdListItem;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIMain_ApplicationProdListItem : WpfListItem
    {
        
        public UIMain_ApplicationProdListItem(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.Name] = "Main_Application.ProductControl";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public UIThisCustom UIThisCustom
        {
            get
            {
                if ((this.mUIThisCustom == null))
                {
                    this.mUIThisCustom = new UIThisCustom(this);
                }
                return this.mUIThisCustom;
            }
        }
        #endregion
        
        #region Fields
        private UIThisCustom mUIThisCustom;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIThisCustom : WpfCustom
    {
        
        public UIThisCustom(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.ClassName] = "Uia.ProductControl";
            this.SearchProperties[PropertyNames.AutomationId] = "This";
            this.WindowTitles.Add("CoatingSchedule");
            #endregion
        }
        
        #region Properties
        public WpfEdit UITxbThicknessEdit
        {
            get
            {
                if ((this.mUITxbThicknessEdit == null))
                {
                    this.mUITxbThicknessEdit = new WpfEdit(this);
                    #region Search Criteria
                    this.mUITxbThicknessEdit.SearchProperties[WpfEdit.PropertyNames.AutomationId] = "TxbThickness";
                    this.mUITxbThicknessEdit.WindowTitles.Add("CoatingSchedule");
                    #endregion
                }
                return this.mUITxbThicknessEdit;
            }
        }
        #endregion
        
        #region Fields
        private WpfEdit mUITxbThicknessEdit;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIHardboardSchedulerWindow : WpfWindow
    {
        
        public UIHardboardSchedulerWindow()
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.Name] = "Hardboard Scheduler";
            this.SearchProperties.Add(new PropertyExpression(PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.WindowTitles.Add("Hardboard Scheduler");
            #endregion
        }
        
        #region Properties
        public UIHardboardSchedulerTitleBar UIHardboardSchedulerTitleBar
        {
            get
            {
                if ((this.mUIHardboardSchedulerTitleBar == null))
                {
                    this.mUIHardboardSchedulerTitleBar = new UIHardboardSchedulerTitleBar(this);
                }
                return this.mUIHardboardSchedulerTitleBar;
            }
        }
        #endregion
        
        #region Fields
        private UIHardboardSchedulerTitleBar mUIHardboardSchedulerTitleBar;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "12.0.21005.1")]
    public class UIHardboardSchedulerTitleBar : WpfTitleBar
    {
        
        public UIHardboardSchedulerTitleBar(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[PropertyNames.AutomationId] = "TitleBar";
            this.WindowTitles.Add("Hardboard Scheduler");
            #endregion
        }
        
        #region Properties
        public WpfButton UICloseButton
        {
            get
            {
                if ((this.mUICloseButton == null))
                {
                    this.mUICloseButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUICloseButton.SearchProperties[WpfButton.PropertyNames.Name] = "Close";
                    this.mUICloseButton.WindowTitles.Add("Hardboard Scheduler");
                    #endregion
                }
                return this.mUICloseButton;
            }
        }
        #endregion
        
        #region Fields
        private WpfButton mUICloseButton;
        #endregion
    }
}