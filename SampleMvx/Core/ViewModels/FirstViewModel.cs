using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;

namespace SampleMvx.Core.ViewModels
{
    public class FirstViewModel
        : MvxViewModel
    {
        public ObservableCollection<PageViewModel> Pages = new ObservableCollection<PageViewModel>();

        private MvxCommand _addPageCommand;

        public ICommand AddPageCommand
        {
            get
            {
                _addPageCommand = _addPageCommand ?? new MvxCommand(DoAddPageCommand);
                return _addPageCommand;
            }
        }

        public int PageCount { get; private set; }

        private void DoAddPageCommand()
        {
            ++PageCount;
            RaisePropertyChanged(() => PageCount);

            Pages.Add(new PageViewModel { Hello = $"Page {PageCount}" });
        }

        private MvxCommand _removePageCommand;

        public ICommand RemovePageCommand
        {
            get
            {
                _removePageCommand = _removePageCommand ?? new MvxCommand(DoRemovePageCommand);
                return _removePageCommand;
            }
        }

        private void DoRemovePageCommand()
        {
            if (!Pages.Any()) return;

            --PageCount;
            RaisePropertyChanged(() => PageCount);

            Pages.Remove(Pages.Last());
        }
    }
}
