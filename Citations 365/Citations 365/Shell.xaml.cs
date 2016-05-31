using citations365.Presentation;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace citations365 {
    public sealed partial class Shell : UserControl
    {
        private static string _header;
        public string Header {
            get {
                if (_header == null) {
                    _header = "ACCUEIL";
                }
                return _header;
            }
            set {
                if (_header != value) {
                    _header = value;
                }
            }
        }

        public Shell()
        {
            InitializeComponent();

            var vm = new ShellViewModel();

            vm.MenuItems.Add(new MenuItem {
                SymbolAsChar = '\uE706',
                Label = "Aujourd'hui",
                PageType = typeof(Views.TodayPage)
            });
            vm.MenuItems.Add(new MenuItem {
                SymbolAsChar = '\uE00B',
                Label = "Favoris",
                PageType = typeof(Views.FavoritesPage)
            });
            vm.MenuItems.Add(new MenuItem {
                SymbolAsChar = '\uE11A',
                Label = "Recherche",
                PageType = typeof(Views.SearchPage)
            });
            vm.MenuItems.Add(new MenuItem {
                SymbolAsChar = '\uE2AF',
                Label = "Auteurs",
                PageType = typeof(Views.AuthorsPage)
            });
            vm.MenuItems.Add(new MenuItem {
                SymbolAsChar = '\uE115',
                Label = "Settings",
                PageType = typeof(Views.SettingsPage)
            });

            // select the first menu item
            vm.SelectedMenuItem = vm.MenuItems.First();

            ViewModel = vm;

            // add entry animations
            var transitions = new TransitionCollection { };
            var transition = new NavigationThemeTransition { };
            transitions.Add(transition);
            Frame.ContentTransitions = transitions;
        }

        public ShellViewModel ViewModel { get; private set; }

        public Frame RootFrame
        {
            get
            {
                if (Frame.SourcePageType != null) {
                    //Header = Frame.SourcePageType.Name;
                    SetHeaderTitle(Frame.SourcePageType.Name);
                }
                
                return Frame;
            }
        }

        private void SetHeaderTitle(string name) {
            switch (name) {
                case "TodayPage":
                    VisualHeader.Text = "ACCEUIL";
                    break;
                case "FavoritesPage":
                    VisualHeader.Text = "FAVORIS";
                    break;
                case "SearchPage":
                    VisualHeader.Text = "RECHERCHE";
                    break;
                case "AuthorsPage":
                    VisualHeader.Text = "AUTEURS";
                    break;
                case "SettingsPage":
                    VisualHeader.Text = "PARAMÈTRES";
                    break;
                default:
                    VisualHeader.Text = name;
                    break;
            }
        }
    }
}
