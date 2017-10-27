using Xamarin.Forms;
using XamarinFormsSuperScroll.Models;

namespace XamarinFormsSuperScroll
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            DependencyService.Register<DataModel>();

            MainPage = new XamarinFormsSuperScrollPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}