using Xamarin.Forms;
using XamarinFormsSuperScroll.ViewModels;

namespace XamarinFormsSuperScroll
{
    public partial class XamarinFormsSuperScrollPage : ContentPage
    {
        public XamarinFormsSuperScrollPage()
        {
            InitializeComponent();

            BindingContext = new VirtualizingListViewModel();
        }
    }
}