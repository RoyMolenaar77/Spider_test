using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Concentrator.Plugins.Wehkamp;

namespace CreateTestFiles
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void buttonSelectShipmentNotificationFile_Click(object sender, EventArgs e)
    {
      var ofd = new OpenFileDialog { Multiselect = false };
      if (ofd.ShowDialog() == DialogResult.OK)
      {
        textBoxShipmentNotificationFile.Text = ofd.FileName;
      }
    }
    private void buttonCreateShipmentConfirmationFile_Click(object sender, EventArgs e)
    {
      CreateShipmentConfirmation(textBoxShipmentNotificationFile.Text);
    }
    private void CreateShipmentConfirmation(string shipmentNotificationFile)
    {
      var aankomst = new aankomst();
      var path = Path.GetDirectoryName(shipmentNotificationFile);

      if (aankomst.LoadFromFile(shipmentNotificationFile, out aankomst))
      {
        var aankomstBevestiging = new aankomstBevestiging();
        aankomstBevestiging.header.berichtNaam = "aankomstBevestiging";
        aankomstBevestiging.header.retailPartnerCode = aankomst.header.retailPartnerCode;
        aankomstBevestiging.header.berichtDatumTijd = DateTime.Now;
        aankomstBevestiging.header.bestandsNaam = aankomst.header.bestandsNaam.Replace(".xml", "Bevestiging.xml");


        foreach (var item in aankomst.aankomsten)
        {
          aankomstBevestiging.aankomst.Add(
            new aankomstBevestigingAankomst
            {
              aantalOntvangen = item.aantalOpgegeven,
              artikelNummer = item.artikelNummer,
              kleurNummer = item.kleurNummer,
              maat = item.maat,
              ggb = item.ggb,
              goederenSoort = "V",
              locusStatus = "RES",
              werkelijkeAankomstDatum = item.verwachteAankomstDatum

            });

        }

        aankomstBevestiging.SaveToFile(Path.Combine(path, aankomstBevestiging.header.bestandsNaam));

      }
      else
      {
        MessageBox.Show(@"Selected file isn't a valid shipment notification file", @"Loading file");
      }

    }



    private void buttonSelectProductInformationFile_Click(object sender, EventArgs e)
    {
      var ofd = new OpenFileDialog { Multiselect = false };
      if (ofd.ShowDialog() == DialogResult.OK)
      {
        textBoxProductInformationFile.Text = ofd.FileName;
      }
    }

    private void buttonCreateProductRelationFile_Click(object sender, EventArgs e)
    {
      CreateProductRelation(textBoxProductInformationFile.Text);
    }
    private void CreateProductRelation(string productInformationFile)
    {
      var artikelInformatie = new artikelInformatie();
      var path = Path.GetDirectoryName(productInformationFile);

      if (artikelInformatie.LoadFromFile(productInformationFile, out artikelInformatie))
      {
        var artikelRelatie = new artikelRelatie();
        artikelRelatie.header.berichtNaam = "artikelRelatie";
        artikelRelatie.header.retailPartnerCode = artikelInformatie.header.retailPartnerCode;
        artikelRelatie.header.berichtDatumTijd = DateTime.Now;
        artikelRelatie.header.bestandsNaam = artikelInformatie.header.bestandsNaam.Replace("artikelInformatie.xml", "artikelRelatie.xml");


        foreach (var item in artikelInformatie.artikel)
        {
           artikelRelatie.relatie.Add(
            new artikelRelatieRelatie
            {
              artikelNummer = item.artikelNummer,
              kleurNummer = item.kleurNummer,
              wehkampArtikelNummer = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)
            });

        }

        artikelRelatie.SaveToFile(Path.Combine(path, artikelRelatie.header.bestandsNaam));

      }
      else
      {
        MessageBox.Show(@"Selected file isn't a valid shipment notification file", @"Loading file");
      }

    }


  }
}
