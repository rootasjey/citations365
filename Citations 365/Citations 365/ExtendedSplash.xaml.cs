using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.Foundation;
using Windows.UI.Xaml;
using citations365.Views;
using System;
using citations365.Models;
using citations365.Controllers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Hosting;
using System.Numerics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace citations365 {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtendedSplash : Page {
        internal Rect splashImageRect; // Rect to store splash screen image coordinates.
        private SplashScreen splash; // Variable to hold the splash screen object.
        internal bool dismissed = false; // Variable to track splash screen dismissal status.
        internal Shell rootFrame;
        UIElement _shellContent;

        public ExtendedSplash(SplashScreen splashscreen, bool loadState, Shell shell, UIElement shellContent) {
            InitializeComponent();
            //LoadSplashImage();
            LoadSplashQuote();
            
            Window.Current.SizeChanged += new WindowSizeChangedEventHandler(ExtendedSplash_OnResize);

            splash = splashscreen;

            if (splash != null) {
                splash.Dismissed += new TypedEventHandler<SplashScreen, Object>(DismissedEventHandler);

                splashImageRect = splash.ImageLocation;
                PositionImage();
                PositionStack();
            }

            // Create a Frame to act as the navigation context
            //rootFrame = new Frame();
            rootFrame = shell;
            _shellContent = shellContent;

            LoadTodayList();
        }

        void PositionImage() {
            splashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
            splashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
            splashImage.Height = splashImageRect.Height;
            splashImage.Width = splashImageRect.Width;
        }

        void PositionRing() {
            splashProgress.SetValue(Canvas.LeftProperty, splashImageRect.X + (splashImageRect.Width * 0.5) - (splashProgress.Width * 0.5));
            splashProgress.SetValue(Canvas.TopProperty, (splashImageRect.Y + splashImageRect.Height + splashImageRect.Height * 0.1));
        }

        void PositionStack() {
            stack.SetValue(Canvas.LeftProperty, splashImageRect.X + (splashImageRect.Width * 0.5) - (stack.Width * 0.5));
            stack.SetValue(Canvas.TopProperty, (splashImageRect.Y + splashImageRect.Height + splashImageRect.Height * 0.1));
        }

        void ExtendedSplash_OnResize(Object sender, WindowSizeChangedEventArgs e) {
            // Safely update the extended splash screen image coordinates. This function will be executed when a user resizes the window.
            if (splash != null) {
                // Update the coordinates of the splash screen image.
                splashImageRect = splash.ImageLocation;
                PositionImage();

                // If applicable, include a method for positioning a progress control.
                //PositionRing();
                PositionStack();
            }
        }

        // Include code to be executed when the system has transitioned from the splash screen to the extended splash screen (application's first view).
        void DismissedEventHandler(SplashScreen sender, object e) {
            dismissed = true;

            // Complete app setup operations here...
        }

        void DismissExtendedSplash() {
            //ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("heroQuote", stackQuote);
            //var matrice = stackQuote.TransformToVisual(Window.Current.Content);
            //Point heroQuoteCoords = matrice.TransformPoint(new Point(0, 0));

            //var visual = ElementCompositionPreview.GetElementVisual(PageCanvas);
            //var compositor = visual.Compositor;

            //var duration = TimeSpan.FromMilliseconds(500);

            //var slideUpAnimation = compositor.CreateVector2KeyFrameAnimation();
            //slideUpAnimation.InsertKeyFrame(0.0f, new Vector2(0f, 0f));
            //slideUpAnimation.InsertKeyFrame(1.0f, new Vector2(0, -500));
            //slideUpAnimation.Duration = duration;

            //var fadeOutAnimation = compositor.CreateScalarKeyFrameAnimation();
            //fadeOutAnimation.InsertKeyFrame(1, 0);
            //fadeOutAnimation.Duration = duration;

            //var batch = compositor.CreateScopedBatch(Windows.UI.Composition.CompositionBatchTypes.Animation);
            //batch.Completed += (sender, args) => {
            //    rootFrame.Content = _shellContent;
            //    App._shell.RootFrame.Navigate(typeof(TodayPage));
            //};

            //visual.StartAnimation("Opacity", fadeOutAnimation);
            //visual.StartAnimation("Offset.xy", slideUpAnimation);
            //batch.End();

            rootFrame.Content = _shellContent;
            App._shell.RootFrame.Navigate(typeof(TodayPage));

            //rootFrame.RootFrame.Navigate(typeof(TodayPage));
            //Window.Current.Content = rootFrame;
        }

        void LoadSplashQuote() {
            Quote lockscreenQuote = TodayController.GetLockScreenQuote();
            if (lockscreenQuote == null) return;

            quoteContent.Text = lockscreenQuote.Content;
            quoteAuthor.Text = lockscreenQuote.Author;
        }

        async void LoadTodayList() {
            TodayController controller = new TodayController();
            await controller.LoadData();
            DismissExtendedSplash();
        }

        async void LoadSplashImage() {
            string url = await TodayController.GetAppBackgroundURL();

            if (!string.IsNullOrEmpty(url)) {
                var bitmap = new BitmapImage(new Uri(url));
                splashImage.Source = bitmap;
            }
        }
    }
}
