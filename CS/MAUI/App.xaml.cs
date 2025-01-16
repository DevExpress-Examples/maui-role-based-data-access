using MAUI.Services;
using MAUI.ViewModels;
using MAUI.Views;


namespace MAUI {
	public partial class App {
		public App() {
			InitializeComponent();
			DependencyService.Register<NavigationService>();
			DependencyService.Register<WebAPIService>();

			Routing.RegisterRoute(typeof(ItemsPage).FullName, typeof(ItemsPage));
			
			var navigationService = DependencyService.Get<INavigationService>();
			navigationService.NavigateToAsync<LoginViewModel>(true);
		}

		protected override Window CreateWindow(IActivationState activationState) {
            return new Window(new AppShell());
        }
	}
}
