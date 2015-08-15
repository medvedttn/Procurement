using System.Windows;
using System.Windows.Controls;
using Procurement.ViewModel;
using System.Windows.Media;
using System;
using POEApi.Model;
using System.Linq;
using System.Globalization;

namespace Procurement.View
{
    public partial class LoginView : UserControl, IView
    {
        public LoginView()
        {
            InitializeComponent();
            this.DataContext = new LoginWindowViewModel(this);
        }

        public new Grid Content
        {
            get { return this.ViewContent; }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as LoginWindowViewModel).Login(false, ((ComboBoxItem)cmbRealmType.SelectedItem).Content.ToString());
        }

        private void Offline_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as LoginWindowViewModel).Login(true, ((ComboBoxItem)cmbRealmType.SelectedItem).Content.ToString());
        }

        private void cmbRealmType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRealmType.SelectedItem != null)
            {
                LoginWindowViewModel.ServerType = ((ComboBoxItem)cmbRealmType.SelectedItem).Content.ToString();
                if (LoginWindowViewModel.ServerType != "International")
                {
                    //Garena servers
                    useSession.IsChecked = false;
                    useSession.IsEnabled = false;
                    lblEmail.IsEnabled = false;
                    txtLogin.Text = "NoEmail";
                    txtLogin.IsEnabled = false;
                    lblPassword.Content = "SessionID";

                    //RU GUI
                    lblEmail.Content = "Эл.почта";
                    lblPassword.Content = "ID сессии";
                    lblScale.Content = "Масштаб окна";

                    ChangeImageStyle(LoginButton.Content as Image, LoginWindowViewModel.ServerType);
                    ChangeImageStyle(OfflineButton.Content as Image, LoginWindowViewModel.ServerType);
                    
                    Procurement.MessagesRes.Culture = System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU");
                }
                else
                {
                    useSession.IsEnabled = true;
                    lblEmail.IsEnabled = true;
                    txtLogin.IsEnabled = true;
                    txtLogin.Text = "";
                    lblEmail.Content = "Email";
                    lblPassword.Content = "Password";
                    ChangeImageStyle(LoginButton.Content as Image, LoginWindowViewModel.ServerType);
                    ChangeImageStyle(OfflineButton.Content as Image, LoginWindowViewModel.ServerType);
                    Procurement.MessagesRes.Culture = System.Globalization.CultureInfo.InvariantCulture;
                }
            }
            else
	        {
                cmbRealmType.SelectedIndex = 0;
	        }
        }

        private void cmbRealmType_Loaded(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Settings.UserSettings["ServerType"]) && Settings.UserSettings["ServerType"] != "")
            {
                cmbRealmType.SelectedItem = cmbRealmType.Items.Cast<ComboBoxItem>().FirstOrDefault(cmbi => cmbi.Content.ToString() == Settings.UserSettings["ServerType"]);
            }

            ChangeImageStyle(LoginButton.Content as Image, LoginWindowViewModel.ServerType);
            ChangeImageStyle(OfflineButton.Content as Image, LoginWindowViewModel.ServerType);
        }

        internal static void ChangeImageStyle(Image button_img, string server_type)
        {
            Style new_style = new Style(typeof(Image), button_img.Style);

            new_style.Triggers.Add(new Trigger() { Value = true, Property = Image.IsMouseOverProperty });
            if (server_type == "Garena (RU)")
            {
                if (!button_img.Source.ToString().Contains("/buttons_ru/"))
                {
                    new_style.Setters.Add(new Setter(Image.SourceProperty, new ImageSourceConverter().ConvertFromString(button_img.Source.ToString().Replace("/buttons/", "/buttons_ru/"))));
                    (new_style.Triggers[0] as Trigger).Setters.Add(new Setter(Image.SourceProperty, new ImageSourceConverter().ConvertFromString(((button_img.Style.Triggers[0] as Trigger).Setters[0] as Setter).Value.ToString().Replace("/buttons/", "/buttons_ru/"))));
                    button_img.Style = new_style;
                }
            }
            else
            {
                if (!button_img.Source.ToString().Contains("/buttons/"))
                {
                    new_style.Setters.Add(new Setter(Image.SourceProperty, new ImageSourceConverter().ConvertFromString(button_img.Source.ToString().Replace("/buttons_ru/", "/buttons/"))));
                    (new_style.Triggers[0] as Trigger).Setters.Add(new Setter(Image.SourceProperty, new ImageSourceConverter().ConvertFromString(((button_img.Style.Triggers[0] as Trigger).Setters[0] as Setter).Value.ToString().Replace("/buttons_ru/", "/buttons/"))));
                    button_img.Style = new_style;
                }
            }
        }

        private void cmbScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbScale.SelectedItem != null)
            {
                float old_scale;
                if (!float.TryParse(Settings.UserSettings["WindowScale"], System.Globalization.NumberStyles.Float, new CultureInfo("en-US"), out old_scale))
                {
                    old_scale = 1.0f;
                }
                string str_scale = ((ComboBoxItem)cmbScale.SelectedItem).Content.ToString().Replace("%","");
                float scale = float.Parse(str_scale)/100.0f;

                if (old_scale != scale)
                {
                    Settings.UserSettings["WindowScale"] = scale.ToString("f", new CultureInfo("en-US"));
                    Settings.Save();
                    MessageBox.Show(Procurement.MessagesRes.RestartApplicationToApplyWindowScaleSetting, "Procurement", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void cmbScale_Loaded(object sender, RoutedEventArgs e)
        {
            float old_scale;
            string[] standard_scales = {"100%","90%","80%","75%","70%"};
            if (!float.TryParse(Settings.UserSettings["WindowScale"], System.Globalization.NumberStyles.Float, new CultureInfo("en-US"), out old_scale))
            {
                old_scale = 1.0f;
            }

            string str_old_scale = (old_scale * 100.0f).ToString()+"%";

            if (standard_scales.Contains(str_old_scale))
            {
                cmbScale.SelectedItem = cmbScale.Items.Cast<ComboBoxItem>().FirstOrDefault(cmbi => cmbi.Content.ToString() == str_old_scale);
            }
            else
            {
                ComboBoxItem newItem= new ComboBoxItem();
                newItem.Content=str_old_scale;
                cmbScale.Items.Add(newItem);
                cmbScale.SelectedItem = newItem;
            }

        }
    }
}
