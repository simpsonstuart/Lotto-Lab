using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Tasks ;

namespace MyLottoLabWinPhone8
{
    public partial class Page1 : PhoneApplicationPage
    {
        public Page1()
        {
            InitializeComponent();

        }

           private void composeMail_Tap(object sender, System.Windows.Input.GestureEventArgs e)
           {
               EmailComposeTask emailComposeTask = new EmailComposeTask();
               emailComposeTask.To = "windowsphone@fusionfjord.com";
               emailComposeTask.Body = "";
               emailComposeTask.Cc = "windowsphone@fusionfjord.com";
               emailComposeTask.Subject = "My Lotto Lab Feedback V1.0.0.4";
               emailComposeTask.Show();
           }
           private void btnback_Click(object sender, RoutedEventArgs e)
           {
               if (NavigationService.CanGoBack)
               {
                   NavigationService.GoBack();
               }
           }

    }
}