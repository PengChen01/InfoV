using InfoV.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InfoV
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      
      dosomething();
    }

    private void dosomething()
    {
      IEnumerable<Fund.Fund> funds = Fund.DataServices.GetFund();
      CreateDashBoard.ProgramedLayout(this,funds);
    }



    

    private void SearchFund_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key== Key.Up||e.Key==Key.Down)
      {
        return;
      }
      CollectionView itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView(SearchFund.ItemsSource);
      itemsViewOriginal.Filter = o =>
      {
        if (String.IsNullOrEmpty(SearchFund.Text))
        {
          return true;
        }
        else
        {
          if (o is Fund.Fund fund)
          {
            if (fund.Code.Contains(SearchFund.Text) || fund.Name.Contains(SearchFund.Text) || fund.Type.Contains(SearchFund.Text))
            {
              SearchFund.IsDropDownOpen = true;
              return true;
            }
            else
            {
              return false;
            }
          }
        }
        return false;
      };
      //itemsViewOriginal.Refresh();

      
      // if datasource is a DataView, then apply RowFilter as below and replace above logic with below one
      /* 
       DataView view = (DataView) Cmb.ItemsSource; 
       view.RowFilter = ("Name like '*" + Cmb.Text + "*'"); 
      */
    }

    private void SearchFund_Selected(object sender, RoutedEventArgs e)
    {
      
    }

    private void SearchFund_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (SearchFund.SelectedItem is Fund.Fund fund)
      {
        Fund.DataServices.GetFundDetail(fund);
        if (fund.Detail!=null)
        {
          CreateDashBoard.RefreshDetail(this, fund);
        }
        
      }
    }
  }
}
