using MMADatabase;
using MMADatabase.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace MMABettingModule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void CleanData(object sender, RoutedEventArgs e)
        {
            using (MMADatabaseModel db = new MMADatabaseModel())
            {
                DateTime dt = new DateTime(1, 1, 1);
                //List<OddsInfo> l = db.oddsInfo.Where(x => Math.Abs(x.OddsValue) > 1200).ToList();
                List<OddsInfo> p = db.oddsInfo.Where(x => x.DateTaken == dt).ToList();
                //db.oddsInfo
                //    .RemoveRange(db.oddsInfo
                //    .Where(x => Math.Abs(x.OddsValue) > 1200));
                MessageBoxResult res = MessageBox.Show("This will remove " + p.Count.ToString() + " entries. Do you wish to continue?", "Confirmation", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes)
                {
                    db.oddsInfo
                  .RemoveRange(db.oddsInfo.Where(x => x.DateTaken == dt));
                    int k = db.SaveChanges();
                    MessageBox.Show(k.ToString() + " Entries Removed");
                }
            }
        }
    }
}