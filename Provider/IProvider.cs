using Aniflex.Services;

using System.Threading.Tasks;

namespace Aniflex.Provider;

public interface IProvider
{
    string Name { get; }
    Task GetShows(WebContext context, ProviderProgress progress);
}
