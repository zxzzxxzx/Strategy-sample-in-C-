﻿#pragma checksum "..\..\SecuritiesWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "35B319B5A935CE87AC94E93770F2B7E8"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.17929
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using Ecng.Xaml.Converters;
using StockSharp.Xaml;
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


namespace Sample {
    
    
    /// <summary>
    /// SecuritiesWindow
    /// </summary>
    public partial class SecuritiesWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\SecuritiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Sample.SecuritiesWindow securitiesWindow;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\SecuritiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView SecuritiesDetails;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\SecuritiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Quotes;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\SecuritiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button NewOrder;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\SecuritiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button NewStopOrder;
        
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
            System.Uri resourceLocater = new System.Uri("/RoboGISMO;component/securitieswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\SecuritiesWindow.xaml"
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
            this.securitiesWindow = ((Sample.SecuritiesWindow)(target));
            return;
            case 2:
            this.SecuritiesDetails = ((System.Windows.Controls.ListView)(target));
            
            #line 27 "..\..\SecuritiesWindow.xaml"
            this.SecuritiesDetails.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.SecuritiesDetailsSelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Quotes = ((System.Windows.Controls.Button)(target));
            
            #line 52 "..\..\SecuritiesWindow.xaml"
            this.Quotes.Click += new System.Windows.RoutedEventHandler(this.QuotesClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.NewOrder = ((System.Windows.Controls.Button)(target));
            
            #line 53 "..\..\SecuritiesWindow.xaml"
            this.NewOrder.Click += new System.Windows.RoutedEventHandler(this.NewOrderClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.NewStopOrder = ((System.Windows.Controls.Button)(target));
            
            #line 54 "..\..\SecuritiesWindow.xaml"
            this.NewStopOrder.Click += new System.Windows.RoutedEventHandler(this.NewStopOrderClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

