﻿using GMS___Model;
using Newtonsoft.Json;
using System;
using MahApps.Metro.Controls;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Navigation;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace GMS___Desktop_Client
{
    /// <summary>
    /// Interaction logic for LogInScreen.xaml
    /// </summary>
    public partial class LogInScreen : MetroWindow
    {
        private readonly HttpClient client;

        public LogInScreen()
        {
            InitializeComponent();
            client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44377/");
            var path = Environment.CurrentDirectory;
            var uriPath = new Uri(path.Substring(0,path.LastIndexOf("bin"))+ @"\Images\bg.png", UriKind.RelativeOrAbsolute);
            this.Resources["BackgroundPath"] = new BitmapImage(uriPath);
        }

        private void logInButton_Click(object sender, RoutedEventArgs e)
        {
            User user = new User() { UserName = userEmailText.Text, Password = passwordText.Password, EmailAddress = ""};

            var login = client.PostAsJsonAsync("api/user/login", user).Result;

            if (login.StatusCode == HttpStatusCode.OK)
            {
                string authToken = login.Content.ReadAsStringAsync().Result;
                client.DefaultRequestHeaders.Add("Authorization", authToken);
                var userInfo = client.GetAsync("api/user").Result;
                User returnUser = JsonConvert.DeserializeObject<User>(userInfo.Content.ReadAsStringAsync().Result);
                App.Current.Properties["AuthToken"] = authToken;
                //TODO if apikey null

                if (returnUser.ApiKey == "")
                {
                    MessageBox.Show("Account does not have an API Key", "Characters", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Set neeed properties for the scope of the application
                SetAppProperties(returnUser);

                //Get User Characters
                GetCharacters(returnUser);
                GetDefaultCharacter();

                var windowLocation = this.PointToScreen(new Point(0, 0));
                Window MainWindow = new MainWindow();
                MainWindow.Left = windowLocation.X;
                MainWindow.Top = windowLocation.Y;
                MainWindow.Show();
                Close();
            } else
            {
                IncorrectCredentialsTextBlock.Text = "Wrong username or password. Try again.";
            }

        }

        private void Hyperlink_ForgotPassword(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Hyperlink_CreateAccount(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void GetCharacters(User returnUser)
        {
            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue((string)App.Current.Properties["ApiKey"]);
                var json = client.GetStringAsync("gw2api/characters").Result;
                ArrayList characters = JsonConvert.DeserializeObject<ArrayList>(json);
                string chars = "";
                foreach (var item in characters)
                {
                    chars = chars + " " + item;
                }
                returnUser.Characters = characters;
                App.Current.Properties["Characters"] = characters;
            } catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void GetDefaultCharacter()
        {
            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue((string)App.Current.Properties["ApiKey"]);
                var json = client.GetStringAsync("gw2api/characters/" + ((ArrayList)App.Current.Properties["Characters"])[0].ToString() + "/core").Result;
                var chara = JsonConvert.DeserializeObject<Character>(json);
                App.Current.Properties["CharacterGuildID"] = chara.Guild;
                App.Current.Properties["SelectedCharacter"] = chara.Name;
                App.Current.Properties["SelectedCharacterObject"] = chara;
            } catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetAppProperties(User returnedUser)
        {
            App.Current.Properties["ApiKey"] = returnedUser.ApiKey;
            App.Current.Properties["UserName"] = returnedUser.UserName;
            App.Current.Properties["Characters"] = returnedUser.Characters;
            //App.Current.Properties["GuildID"] = "821239B1-FE78-3742-83A4-75152E1ED7A96C18AA79-9093-490B-8B9A-F2AA6C8DAB8E";
            App.Current.Properties["SelectedCharacter"] = "";
        }
    }
}
