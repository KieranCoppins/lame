using Lame.Frontend.Enums;

namespace Lame.Frontend.ViewModels;

public class PageViewModel : BaseViewModel
{
    private AppPage _page;
    public AppPage Page
    {
        get => _page;

        protected set
        {
            if (_page == value)
            {
                return;
            }
            
            _page = value;
            OnPropertyChanged();
        }
    }
}