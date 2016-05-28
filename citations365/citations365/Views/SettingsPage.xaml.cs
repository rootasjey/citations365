using BackgroundTasks.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BackgroundTasks.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private static SettingsController _Scontroller;

        public static SettingsController Scontroller {
            get {
                if (_Scontroller == null) {
                    _Scontroller = new SettingsController();
                }
                return _Scontroller;
            }
        }
        public SettingsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Add or remove background task when the toggle changes state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskSwitch_Toggled(object sender, RoutedEventArgs e) {
            var toggle = (ToggleSwitch)sender;
            if (toggle.IsOn) {
                Scontroller.RegisterBackgroundTask();
            } else {
                Scontroller.UnregisterBackgroundTask();
            }
        }
    }
}
