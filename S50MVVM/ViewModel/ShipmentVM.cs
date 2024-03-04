using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S50MVVM.Model;

namespace S50MVVM.ViewModel
{
    class ShipmentVM: Utilities.ViewModelBase
    {
        private readonly PageModel _pageModel;
        public DateTime ShipmentTracking
        {
            get { return _pageModel.ShipmentDelivery; }
            set { _pageModel.ShipmentDelivery = value; OnPropertyChanged(); }
        }

        public ShipmentVM()
        {
            _pageModel = new PageModel();
            ShipmentTracking = DateTime.Now;
        }
    }
}
