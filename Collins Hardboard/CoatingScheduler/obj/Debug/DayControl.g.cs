﻿#pragma checksum "..\..\DayControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "89BBF8668DF17640D0805B2E0C06DA5D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using CoatingScheduler;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace CoatingScheduler {
    
    
    /// <summary>
    /// DayControl
    /// </summary>
    public partial class DayControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 11 "..\..\DayControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker DayDatePicker;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\DayControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnRemoveDay;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\DayControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnRunDay;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\DayControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView LineListView;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\DayControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnAddLine;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/CoatingScheduler;component/daycontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\DayControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.DayDatePicker = ((System.Windows.Controls.DatePicker)(target));
            
            #line 11 "..\..\DayControl.xaml"
            this.DayDatePicker.SelectedDateChanged += new System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(this.DayDatePicker_OnSelectedDateChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.BtnRemoveDay = ((System.Windows.Controls.Button)(target));
            
            #line 12 "..\..\DayControl.xaml"
            this.BtnRemoveDay.Click += new System.Windows.RoutedEventHandler(this.RemoveDayButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.BtnRunDay = ((System.Windows.Controls.Button)(target));
            
            #line 13 "..\..\DayControl.xaml"
            this.BtnRunDay.Click += new System.Windows.RoutedEventHandler(this.RunDayButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.LineListView = ((System.Windows.Controls.ListView)(target));
            return;
            case 5:
            this.BtnAddLine = ((System.Windows.Controls.Button)(target));
            
            #line 18 "..\..\DayControl.xaml"
            this.BtnAddLine.Click += new System.Windows.RoutedEventHandler(this.Add_Button);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

