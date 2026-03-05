using Lame.Frontend.Enums;

namespace Lame.Frontend.ViewModels;

public abstract class PageViewModel : BaseViewModel
{
    public AppPage Page
    {
        get;
        protected set => SetField(ref field, value);
    }

    public virtual void OnNavigatedTo()
    {
    }

    public virtual void OnNavigatedFrom()
    {
    }
}