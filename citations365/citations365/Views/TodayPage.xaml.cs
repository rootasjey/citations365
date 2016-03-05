using citations365.Controllers;
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

namespace citations365.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class TodayPage : Page
    {
        /// <summary>
        /// Today controller
        /// </summary>
        private TodayController Tcontroller;

        public TodayPage() {
            this.InitializeComponent();
            populate();
        }

        /// <summary>
        /// Fill the page with data
        /// </summary>
        public async void populate() {
            Tcontroller = new TodayController();
            await Tcontroller.GetTodayQuotes();

            if (Tcontroller.IsDataLoaded()) {
                // Set the binding, show the list
            } else {
                // Hide the list, show the empty view
            }
        }
    }
}
