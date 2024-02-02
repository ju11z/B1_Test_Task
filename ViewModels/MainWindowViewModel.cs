using B1_Test_Task.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_Test_Task.ViewModels
{
    class MainWindowViewModel: BaseViewModel
    {
        public Task1ViewModel Task1ViewModel { get; set; }
        public Task2ViewModel Task2ViewModel { get; set; }

        public MainWindowViewModel()
        {
            Task1ViewModel = new Task1ViewModel();
            Task2ViewModel = new Task2ViewModel();
        }
    }
}
