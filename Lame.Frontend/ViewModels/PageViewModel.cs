using Lame.Frontend.Enums;

namespace Lame.Frontend.ViewModels;

public class PageViewModel : BaseViewModel
{
    public AppPage Page
    {
        get;
        protected set => SetField(ref field, value);
    }
}