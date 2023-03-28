using System.Collections.ObjectModel;
using MAUI.Models;
using MAUI.Views;

namespace MAUI.ViewModels {
    public class ItemsViewModel : BaseViewModel {
        bool? _canDeletePosts;
        ObservableCollection<Post> _posts;
        ApplicationUser _currentUser;

        public ItemsViewModel() {
            DeletePostCommand = new Command<Post>(DeletePost);
            LogoutCommand = new Command(Logout);
            ExecuteLoadItems();
            GetCurrentUser();
        }

        public ObservableCollection<Post> Posts { get => _posts; set => SetProperty(ref _posts, value); }

        public Command DeletePostCommand { get; }
        public Command LogoutCommand { get; }

        public ApplicationUser CurrentUser { get => _currentUser; set => SetProperty(ref _currentUser, value); }
        public bool CanDeletePosts {
            get {
                if (_canDeletePosts == null) {
                    UpdateCanDeletePostsAsync();
                    return false;
                }
                return _canDeletePosts.Value;
            }
            set {
                _canDeletePosts = value;
                OnPropertyChanged();
            }
        }
        public void Logout() {
            Navigation.NavigateToAsync<LoginViewModel>(isAbsoluteRoute:true);
        }
        public async void DeletePost(Post post) {
            bool isDeleted = await DataStore.DeletePostAsync(post.PostId);
            if (!isDeleted) {
                await Shell.Current.DisplayAlert("Error", "Couldn't delete the post", "Ok");
            }
            else {
                Posts.Remove(post);
            }
        }
        public async void UpdateCanDeletePostsAsync() {
            CanDeletePosts = await DataStore.UserCanDeletePostAsync();
        }

        async void ExecuteLoadItems() {
            try {
                Posts = new ObservableCollection<Post>(await DataStore.GetItemsAsync(true));
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
        async void GetCurrentUser() {
            try {
                CurrentUser = await DataStore.CurrentUser();
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}