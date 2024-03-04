using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S50MVVM.Model;

namespace S50MVVM.ViewModel
{
    class OrderVM: Utilities.ViewModelBase
    {
        private readonly PageModel _pageModel2;
        public string OrderAvailability
        {
            get { return _pageModel2.OrderStatus; }
            set { _pageModel2.OrderStatus = value; OnPropertyChanged(); }
        }

        public OrderVM()
        {
            _pageModel2 = new PageModel();
            OrderAvailability = "";
        }


    }
}
