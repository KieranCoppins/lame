using System.Threading.Tasks;
using Lame.Frontend.Enums;

namespace Lame.Frontend.ViewModels;

public abstract class PageViewModel : BaseViewModel
{
    public AppPage Page
    {
        get;
        protected set => SetField(ref field, value);
    }

    public virtual Task OnNavigatedTo()
    {
        return Task.CompletedTask;
    }

    public virtual Task OnNavigatedFrom()
    {
        return Task.CompletedTask;
    }
}