using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using POEApi.Model;
using Procurement.View;
using System.Globalization;

namespace Procurement.ViewModel
{
    public class ScreenController
    {
        private static MainWindow mainView;
        private static Dictionary<string, IView> screens = new Dictionary<string, IView>();

        public double HeaderHeight { get; set; }
        public double FooterHeight { get; set; }
        public bool ButtonsVisible{ get; set; }
        public bool FullMode { get; set; }

        public DelegateCommand MenuButtonCommand { get; set; }

        private const string STASH_VIEW = "StashView";
        private const string RECIPE_VIEW = "Recipes";
        private const string TRADING_VIEW = "Trading";
        private const string INVENTORY_VIEW = "Inventory";
        private const string SETTINGS_VIEW = "Settings";
        private const string ABOUT_VIEW = "About";

        public static ScreenController Instance = null;

        public static void Create(MainWindow layout)
        {
            Instance = new ScreenController(layout);
        }

        private ScreenController(MainWindow layout)
        {
            FullMode = !bool.Parse(Settings.UserSettings["CompactMode"]);
            if (FullMode)
            {
                HeaderHeight = 169;
                FooterHeight = 138;
            }

            double Scale=1.0;
            if (Settings.UserSettings.ContainsKey("WindowScale"))
            {
                if (!double.TryParse(Settings.UserSettings["WindowScale"], System.Globalization.NumberStyles.Float, new CultureInfo("en-US"), out Scale))
                {
                    Scale = 1.0;
                }
            }
            else
            {
                Settings.UserSettings.Add("WindowScale", "1.0");
                Settings.Save();
            }

            if (Scale <= 0 || Scale > 10) Scale = 1.0;
            layout.AppScaleTransform.ScaleX = Scale;
            layout.AppScaleTransform.ScaleY = Scale;

            MenuButtonCommand = new DelegateCommand(execute);
            mainView = layout;
            initLogin();
            
        }

        public void UpdateTrading()
        {
            screens[TRADING_VIEW] = new TradingView();
        }

        private void execute(object obj)
        {
            var key = obj.ToString();

            if (key == RECIPE_VIEW && screens[key] == null)
                screens[key] = new RecipeView();

            if (key == TRADING_VIEW && screens[key] == null)
                screens[key] = new TradingView();

            
            LoadView(screens[key]);
        }

        private void initScreens()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
            new Action(() =>
            {
                screens.Add(STASH_VIEW, new StashView());
                screens.Add(INVENTORY_VIEW, new InventoryView());
                screens.Add(TRADING_VIEW, null);
                screens.Add(SETTINGS_VIEW, new SettingsView());
                screens.Add(RECIPE_VIEW, null);
                screens.Add(ABOUT_VIEW, new AboutView());
            }));
        }

        public void InvalidateRecipeScreen()
        {
            screens[RECIPE_VIEW] = null;
        }

        private void initLogin()
        {
            var loginView = new LoginView();
            var loginVM = (loginView.DataContext as LoginWindowViewModel);
            loginVM.OnLoginCompleted += new LoginWindowViewModel.LoginCompleted(loginCompleted);
            LoadView(loginView);
        }

        void loginCompleted()
        {
            initScreens();
            LoadView(screens.First().Value);
            showMenuButtons();
        }

        private static void showMenuButtons()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            new Action(() =>
            {
                mainView.Buttons.Visibility = Visibility.Visible;
                
                //Translate GUI main buttons
                Procurement.View.LoginView.ChangeImageStyle(mainView.btnStash.Content as Image, LoginWindowViewModel.ServerType);
                Procurement.View.LoginView.ChangeImageStyle(mainView.btnInventory.Content as Image, LoginWindowViewModel.ServerType);
                Procurement.View.LoginView.ChangeImageStyle(mainView.btnTrading.Content as Image, LoginWindowViewModel.ServerType);
                Procurement.View.LoginView.ChangeImageStyle(mainView.btnSettings.Content as Image, LoginWindowViewModel.ServerType);
                Procurement.View.LoginView.ChangeImageStyle(mainView.btnRecipes.Content as Image, LoginWindowViewModel.ServerType);
            }));
        }

        public void LoadView(IView view)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, 
                new Action(() => 
                {
                     mainView.MainRegion.Children.Clear();
                     if (view is StashView)
                         screens[STASH_VIEW] = new StashView();
                     mainView.MainRegion.Children.Add(view as UserControl);
                }));
        }
    }
}