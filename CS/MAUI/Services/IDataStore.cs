using MAUI.Models;

namespace MAUI.Services {
    public interface IDataStore<T> {
        Task<T> GetItemAsync(string id);

        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);

        Task<bool> UserCanDeletePostAsync();
        Task<bool> DeletePostAsync(int postId);
        Task<string> Authenticate(string userName, string password);
        Task<ApplicationUser> CurrentUser();
    }
}