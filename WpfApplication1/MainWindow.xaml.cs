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


using System.Data;
using System.IO;
using Excel;

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
            if (string.IsNullOrWhiteSpace(uiEditItemName.Text))
            {
                MessageBox.Show("error");
            }
            else
            {
                AddItem(uiEditItemName.Text, uiEditItemDescription.Text, uiEditItemCatCode.Text);
            }
        }

        private void uiEditRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRow.ItemID == -1)
            {
                MessageBox.Show("Nije izabrana stavka");
            }
            else if (uiEditRowDate.SelectedDate == null)
            {
                MessageBox.Show("Nije izabran datum");
            }
            else
            {
                string Stavka = SelectedRow.Stavka;
                double Rabat = 0, Izlaz = 0, Ulaz = 0;
                if (!string.IsNullOrWhiteSpace(uiEditRowRabat.Text))
                {
                    Rabat = double.Parse(uiEditRowRabat.Text);
                }
                if (!string.IsNullOrWhiteSpace(uiEditRowOutPrice.Text))
                {
                    Izlaz = double.Parse(uiEditRowOutPrice.Text);
                }
                if (!string.IsNullOrWhiteSpace(uiEditRowInPrice.Text))
                {
                    Ulaz = double.Parse(uiEditRowInPrice.Text);
                }
                AddEntry(Stavka, Ulaz, Izlaz, Rabat, uiEditRowVendor.Text, uiEditRowComment.Text, uiEditRowDate.SelectedDate.Value);
            }
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
            if (SelectedRow == null || SelectedRow.ItemID != -1)
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

        private void AddItem( string Stavka, string Opis, string InvKod)
        {
            var row = from foo in BazaPodataka.Stavkes
                      select foo;
            Stavke newInput = new Stavke();
            newInput.Stavka = Stavka;
            newInput.Opis = Opis;
            newInput.Inventurni_kod = InvKod;
            BazaPodataka.Stavkes.Add(newInput);
            BazaPodataka.SaveChanges();
            RefreshSearch();
            RefreshItems();
        }

        private void AddEntry(string stavka, double ulaz, double izlaz, double rabat, string vendor, string komentar, DateTime datum)
        {
            Unosi newInput = new Unosi();
            newInput.Izlazna_cena = izlaz;
            newInput.Ulazna_cena = ulaz;

            if (BazaPodataka.Stavkes.Any(i => i.Stavka == stavka))
            {
                var bar = BazaPodataka.Stavkes.FirstOrDefault(i => i.Stavka == stavka);
                newInput.ItemID = bar.ItemID;
                bar.Izlaz = (izlaz == 0) ? bar.Izlaz : izlaz; //if izlaz == 0, return old value, if new value, return new value
                bar.Ulaz = (ulaz == 0) ? bar.Ulaz : ulaz;
            }           

            newInput.Rabat = rabat;
            newInput.Dobavljac = vendor;
            newInput.Komentari = komentar;
            newInput.Datum = datum;
            BazaPodataka.Unosis.Add(newInput);

            BazaPodataka.SaveChanges();
            RefreshSearch();
            RefreshEntries();
            RefreshItems();
        }

        private void uiEditItemDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (uiEditItemDatagrid.SelectedItem != null)
            {
                SelectedRow = (ItemRow)uiEditItemDatagrid.SelectedItem;
            }            
            RefreshEntries();
        }

        private void uiFileImport_Click(object sender, RoutedEventArgs e)
        {
            XMLimporter window = new XMLimporter();
            string filename = window.filename;

            FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read);


            IExcelDataReader excelReader;
            if (System.IO.Path.GetExtension(filename).ToUpper() == ".XLS")
            {   //1.1 Reading from a binary Excel file ('97-2003 format; *.xls)
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            else
            {   //1.2 Reading from a OpenXml Excel file (2007 format; *.xlsx)
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }

            DataSet result = excelReader.AsDataSet(); 
            
            int rowsMax = result.Tables[0].Rows.Count;
            int colsMax = result.Tables[0].Columns.Count;
            List<ImportRow> noveStavke = new List<ImportRow>();
            for (int i = 0; i < rowsMax; i++)
            {
                if (!string.IsNullOrWhiteSpace(result.Tables[0].Rows[i][0].ToString()))
                {
                    double parseContainer= 0;
                    ImportRow newInput = new ImportRow();
                    newInput.Stavka = result.Tables[0].Rows[i][0].ToString();
                    newInput.Opis = result.Tables[0].Rows[i][1].ToString();
                    newInput.InvKod = result.Tables[0].Rows[i][2].ToString();
                    newInput.Ulaz = Double.TryParse(result.Tables[0].Rows[i][3].ToString(), out parseContainer) ? parseContainer : 0; //if parsable, use. else is 0
                    newInput.Rabat = Double.TryParse(result.Tables[0].Rows[i][4].ToString(), out parseContainer) ? parseContainer : 0;
                    newInput.Izlaz = Double.TryParse(result.Tables[0].Rows[i][5].ToString(), out parseContainer) ? parseContainer : 0; 
                    newInput.Dobavljac = result.Tables[0].Rows[i][6].ToString();
                    newInput.Komentari = result.Tables[0].Rows[i][7].ToString();
                    noveStavke.Add(newInput);
                }
            }
            uiImportDataGrid.ItemsSource = noveStavke;
            excelReader.Close();
        }

        private void uiFileConfirm_Click(object sender, RoutedEventArgs e)
        {
            foreach (ImportRow x in uiImportDataGrid.ItemsSource)
            {
                if (!BazaPodataka.Stavkes.Any(g => g.Stavka == x.Stavka))
                {
                    AddItem(x.Stavka, x.Opis, x.InvKod);
                }
                else
                {
                    var Stavka = BazaPodataka.Stavkes.FirstOrDefault(i => i.Stavka == x.Stavka);
                }

                double Rabat = 0, Izlaz = 0, Ulaz = 0;
                if (x.Rabat.HasValue)
                {
                    Rabat = (double)x.Rabat;
                }
                if (x.Izlaz.HasValue)
                {
                    Izlaz = (double)x.Izlaz;
                }
                if (x.Ulaz.HasValue)
                {
                    Ulaz = (double)x.Ulaz;
                }
                AddEntry(x.Stavka, Ulaz, Izlaz, Rabat, "", "", DateTime.Today); //add more into GUI
            }
        }

        private void uiSearchButton_Click(object sender, RoutedEventArgs e)
        {
            var query = BazaPodataka.Stavkes.Select(foo => new displayRow
            {
                Stavka = foo.Stavka,
                Ulaz = foo.Ulaz,
                Opis = foo.Opis,
                Izlaz = foo.Izlaz,
                InvKod = foo.Inventurni_kod
            }).Where(x => ( x.Stavka.Contains(uiSearchTextbox.Text) ||
                            x.Opis.Contains(uiSearchTextbox.Text) ||
                            x.InvKod.Contains(uiSearchTextbox.Text)
                            ));
            uiSearchDataGrid.ItemsSource = query.ToList();
        }

        private void uiEditSearchButton_Click(object sender, RoutedEventArgs e)
        {
            var query = BazaPodataka.Stavkes.Select(foo => new displayRow
            {
                Stavka = foo.Stavka,
                Ulaz = foo.Ulaz,
                Opis = foo.Opis,
                Izlaz = foo.Izlaz,
                InvKod = foo.Inventurni_kod
            }).Where(x => (x.Stavka.Contains(uiEditSearchTextbox.Text) ||
                            x.Opis.Contains(uiEditSearchTextbox.Text) ||
                            x.InvKod.Contains(uiEditSearchTextbox.Text)
                            ));
            uiEditItemDatagrid.ItemsSource = query.ToList();
        }
    }
}

public class ImportRow //for bulk data import, just a container
{
    public string Stavka { get; set; }
    public string Opis { get; set; }
    public string InvKod { get; set; }
    public string Dobavljac { get; set; }
    public double? Ulaz { get; set; }
    public double? Izlaz { get; set; }
    public double? Rabat { get; set; }
    public string Komentari { get; set; }
}

public class displayRow //for data display (has no ID column)
{
    public string Stavka { get; set; }
    public string Opis { get; set; }
    public string InvKod { get; set; }
    public double? Ulaz { get; set; }
    public double? Izlaz { get; set; }
}

public class ItemRow {
    public long ItemID { get; set; }
    public string Stavka { get; set; }
    public string Opis { get; set; }
    public string InvKod { get; set; }
    public double? Ulaz { get; set; }
    public double? Izlaz { get; set; }
}

public class Entry
{
    public long PrimKey { get; set; }
    public long ItemID { get; set; }
    public string Dobavljac { get; set; }
    public double? Ulaz { get; set; }
    public double? Izlaz { get; set; }
    public double? Rabat { get; set; }
    public string Komentari { get; set; }
    public DateTime Datum { get; set; }
}


//todo
//-dynamic editing
//-autotagger