using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Katalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KatalogEntities BazaPodataka = new KatalogEntities();

        ItemRow SelectedRow = new ItemRow();
        public MainWindow()
        {
            InitializeComponent();
            SelectedRow.ItemID = -1; // -1 is "no row selected"
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshSearch();
            RefreshItems();
            RefreshEntries();
        }
        private void uiEditItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }

        private void uiEditRowButton_Click(object sender, RoutedEventArgs e)
        {
            AddEntry();
        }

        private void RefreshSearch()
        {
            var query = from foo in BazaPodataka.Stavkes
                        where foo.Stavka != null
                        select foo;

            uiSearchDataGrid.ItemsSource = query.ToList();
        }

        private void RefreshItems()
        {

            var query = BazaPodataka.Stavkes.Select(foo => new ItemRow
                             {
                                ItemID = foo.ItemID,
                                Stavka = foo.Stavka,
                                Opis = foo.Opis,
                                InvKod = foo.Inventurni_kod,
                                Ulaz = foo.Ulaz,
                                Izlaz = foo.Izlaz
                             });

            uiEditItemDatagrid.ItemsSource = query.ToList();
        }

        private void RefreshEntries()
        {
            if (SelectedRow.ItemID != -1)
            {
                var query = BazaPodataka.Unosis.Select(foo => new Entry
                {
                    PrimKey = foo.PrimKey,
                    ItemID = foo.ItemID,
                    Dobavljac = foo.Dobavljac,
                    Ulaz = foo.Ulazna_cena,
                    Izlaz = foo.Izlazna_cena,
                    Rabat = foo.Rabat,
                    Komentari = foo.Komentari,
                    Datum = foo.Datum
                }).Where(foo => foo.ItemID == SelectedRow.ItemID);

                uiEditRowDatagrid.ItemsSource = query.ToList();
            }
        }

        private void AddItem()
        {
            if (string.IsNullOrWhiteSpace(uiEditItemName.Text))
            {
                MessageBox.Show("error");
            }
            else
            {
                var row = from foo in BazaPodataka.Stavkes
                          select foo;
                Stavke newInput = new Stavke();
                newInput.Stavka = uiEditItemName.Text;
                newInput.Opis = uiEditItemDescription.Text;
                newInput.Inventurni_kod = uiEditItemCatCode.Text;
                BazaPodataka.Stavkes.Add(newInput);
                BazaPodataka.SaveChanges();
                RefreshSearch();
                RefreshItems();
                RefreshEntries();

            }
        }

        private void AddEntry()
        {
            if (SelectedRow.ItemID == -1)
            {
                MessageBox.Show("error");
            }
            else
            {
                var bar = BazaPodataka.Stavkes.FirstOrDefault(i => i.Stavka == SelectedRow.Stavka);

                var row = from foo in BazaPodataka.Unosis
                          select foo;
                Unosi newInput = new Unosi();

                if (!string.IsNullOrWhiteSpace(uiEditRowInPrice.Text))
                {
                    bar.Ulaz = decimal.Parse(uiEditRowInPrice.Text);
                    newInput.Ulazna_cena = decimal.Parse(uiEditRowInPrice.Text);
                }
                if (!string.IsNullOrWhiteSpace(uiEditRowOutPrice.Text))
                {
                    bar.Izlaz = decimal.Parse(uiEditRowOutPrice.Text);
                    newInput.Izlazna_cena = decimal.Parse(uiEditRowOutPrice.Text);
                }
                if (!string.IsNullOrWhiteSpace(uiEditRowOutPrice.Text))
                {
                    newInput.Rabat = decimal.Parse(uiEditRowRabat.Text);
                }
                
                newInput.ItemID = bar.ItemID;
                newInput.Komentari = uiEditRowComment.Text;
                newInput.Datum = uiEditRowDate.SelectedDate.Value;
                BazaPodataka.Unosis.Add(newInput);

                BazaPodataka.SaveChanges();
                RefreshSearch();
                RefreshItems();
                RefreshEntries();
            }
        }

        private void uiEditItemDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ItemRow SelectedItem = (ItemRow)uiEditItemDatagrid.SelectedItem;
            SelectedRow = (ItemRow)uiEditItemDatagrid.SelectedItem;
            Console.WriteLine(SelectedRow.ItemID);
            RefreshEntries();
        }

    }
}

public class ItemRow {
    public long ItemID { get; set; }
    public string Stavka { get; set; }
    public string Opis { get; set; }
    public string InvKod { get; set; }
    public decimal? Ulaz { get; set; }
    public decimal? Izlaz { get; set; }
}

public class Entry
{
    public long PrimKey { get; set; }
    public long ItemID { get; set; }
    public string Dobavljac { get; set; }
    public decimal? Ulaz { get; set; }
    public decimal? Izlaz { get; set; }
    public decimal? Rabat { get; set; }
    public string Komentari { get; set; }
    public DateTime Datum { get; set; }

}


