﻿#pragma checksum "..\..\PressScheduleWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5C3820C2C8A901AD3A33D17FA93D9A5A"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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


namespace ProductionScheduler {
    
    
    /// <summary>
    /// PressScheduleWindow
    /// </summary>
    public partial class PressScheduleWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 5 "..\..\PressScheduleWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel MainPanel;
        
        #line default
        #line hidden
        
        
        #line 6 "..\..\PressScheduleWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Menu FileMenu;
        
        #line default
        #line hidden
        
        
        #line 7 "..\..\PressScheduleWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem SaveMenuItem;
        
        #line default
        #line hidden
        
        
        #line 8 "..\..\PressScheduleWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem LoadMenuItem;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\PressScheduleWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem Export;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\PressScheduleWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem SettingsMenuItem;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\PressScheduleWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AddItemButton;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\PressScheduleWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView WeekControlListView;
        
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
            System.Uri resourceLocater = new System.Uri("/ProductionScheduler;component/pressschedulewindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\PressScheduleWindow.xaml"
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
            this.MainPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 2:
            this.FileMenu = ((System.Windows.Controls.Menu)(target));
            return;
            case 3:
            this.SaveMenuItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 7 "..\..\PressScheduleWindow.xaml"
            this.SaveMenuItem.Click += new System.Windows.RoutedEventHandler(this.SaveMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.LoadMenuItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 8 "..\..\PressScheduleWindow.xaml"
            this.LoadMenuItem.Click += new System.Windows.RoutedEventHandler(this.LoadMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Export = ((System.Windows.Controls.MenuItem)(target));
            
            #line 9 "..\..\PressScheduleWindow.xaml"
            this.Export.Click += new System.Windows.RoutedEventHandler(this.ExportMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.SettingsMenuItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 10 "..\..\PressScheduleWindow.xaml"
            this.SettingsMenuItem.Click += new System.Windows.RoutedEventHandler(this.SettingsMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 7:
            this.AddItemButton = ((System.Windows.Controls.Button)(target));
            
            #line 17 "..\..\PressScheduleWindow.xaml"
            this.AddItemButton.Click += new System.Windows.RoutedEventHandler(this.AddItemButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 8:
            this.WeekControlListView = ((System.Windows.Controls.ListView)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
