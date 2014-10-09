using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Concentrator.Objects.Models.Forms;

namespace Concentrator.ui.Management.Models
{
  public class CommentsModel
  {
    [Required(ErrorMessage="Verplicht veld!")]
    [Display(Name="Naam medewerker")]
    public string EmployeeName { get; set; }

    [Required(ErrorMessage="Verplicht veld!")]
    [Display(Name ="Prioriteit")]
    public string Priority { get; set; }

    [Display(Name = "Artikelnummer")]
    [Required(ErrorMessage = "Verplicht veld!")]
    public string ArticleNumber { get; set; }

    [Display(Name = "Productnaam")]
    [Required(ErrorMessage = "Verplicht veld!")]
    public string ProductName { get; set; }

    [Display(Name = "Omschrijving fout/wijziging")]
    [Required(ErrorMessage = "Verplicht veld!")]
    public string Description { get; set; }

    [Display(Name = "Melding")]
    [Required(ErrorMessage = "Verplicht veld!")]
    public FormNotificationType Notification { get; set; }
  }
}