﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToolChange.Services;
using ToolChange.ViewModels;

namespace ToolChange.Views.ControlScriptPage
{
    /// <summary>
    /// Interaction logic for TextPage.xaml
    /// </summary>
    public partial class TextPage : Page
    {
        public TextPage()
        {
            InitializeComponent();
           
            DataContext = ViewModelLocator.ScriptVM;
        }

      
    }
}
