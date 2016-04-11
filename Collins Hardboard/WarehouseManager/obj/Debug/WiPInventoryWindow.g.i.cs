﻿#pragma checksum "..\..\WiPInventoryWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8CA56F329A7E4A3C4E20FE4634610533"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ImportLib;
using ModelLib;
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


namespace WarehouseManager {
    
    
    /// <summary>
    /// WiPInventoryWindow
    /// </summary>
    public partial class WiPInventoryWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\WiPInventoryWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal WarehouseManager.WiPInventoryWindow This;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\WiPInventoryWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem SaveItem;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\WiPInventoryWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem LoadItem;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\WiPInventoryWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem ImportItem;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\WiPInventoryWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView WiPItemView;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\WiPInventoryWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AddWiPButton;
        
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
            System.Uri resourceLocater = new System.Uri("/WarehouseManager;component/wipinventorywindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\WiPInventoryWindow.xaml"
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
            this.This = ((WarehouseManager.WiPInventoryWindow)(target));
            return;
            case 2:
            this.SaveItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 12 "..\..\WiPInventoryWindow.xaml"
            this.SaveItem.Click += new System.Windows.RoutedEventHandler(this.SaveItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.LoadItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 13 "..\..\WiPInventoryWindow.xaml"
            this.LoadItem.Click += new System.Windows.RoutedEventHandler(this.LoadItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ImportItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 14 "..\..\WiPInventoryWindow.xaml"
            this.ImportItem.Click += new System.Windows.RoutedEventHandler(this.ImportItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.WiPItemView = ((System.Windows.Controls.ListView)(target));
            return;
            case 6:
            this.AddWiPButton = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\WiPInventoryWindow.xaml"
            this.AddWiPButton.Click += new System.Windows.RoutedEventHandler(this.AddWiPButton_OnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

