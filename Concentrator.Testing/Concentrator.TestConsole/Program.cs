using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Collections.Generic;
using Ninject;
using System.Data.Objects;
using Concentrator.Objects.DataAccess.EntityFramework;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.Models.Users;
using System.Linq;
using Concentrator.Objects.DependencyInjection.NinjectModules;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Microsoft.Practices.ServiceLocation;
using CommonServiceLocator.NinjectAdapter;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Models.Orders;
using System.Data.Linq;
using System.Linq.Dynamic;
using Concentrator.Objects.Models.EDI.Mapping;
using Concentrator.Objects.Models.WebToPrint;
using System.Drawing;
using Concentrator.Objects.Drawing;
using Concentrator.Objects.Images;

namespace Concentrator.Host
{
  public class Program
  {
    static void Main(string[] args)
    {
      Image m = ImageUtility.ResizeImage(Image.FromFile(@"C:\ConcentratorFTP\Products\Media\8_2855098_3_51.jpg"), 50, 50);
      Image m1 = ImageUtility.ResizeImage(Image.FromFile(@"C:\ConcentratorFTP\Products\Media\7_2855099_3_51.jpg"), 50, 50);
      Image m2 = ImageUtility.ResizeImage(Image.FromFile(@"C:\ConcentratorFTP\Products\Media\11_2855095_3_51.jpg"), 50, 50);
      Image m3 = ImageUtility.ResizeImage(Image.FromFile(@"C:\ConcentratorFTP\Products\Media\21_2855085_3_51.jpg"), 50, 50);
      Image m4 = ImageUtility.ResizeImage(Image.FromFile(@"C:\ConcentratorFTP\Products\Media\19_2855087_4_51.jpg"), 120, 120);


      List<SpriteMapModel> model = new List<SpriteMapModel>(){
        new SpriteMapModel(){
          Height = 50,
          Width = 50,
          Path = @"C:\ConcentratorFTP\Products\Media\8_2855098_3_51.jpg",
          Image = m
        },

        new SpriteMapModel(){
          Height = 50,
          Width = 50,
          Path = @"C:\ConcentratorFTP\Products\Media\7_2855099_3_51.jpg",
          Image = m1
        },

        new SpriteMapModel(){
          Height = 50,
          Width = 50,
          Path =@"C:\ConcentratorFTP\Products\Media\11_2855095_3_51.jpg",
          Image = m2
        },

        new SpriteMapModel(){
          Height = 50,
          Width = 50,
          Path = @"C:\ConcentratorFTP\Products\Media\21_2855085_3_51.jpg",
          Image = m3
        },

        new SpriteMapModel(){
          Height = 120,
          Width = 120,
          Path =@"C:\ConcentratorFTP\Products\Media\19_2855087_4_51.jpg",
          Image = m4
        },
      };

      var map = new SpriteMapBuilder(model).BuildMap();
      Sprite spr = new Sprite(map);
      var im = spr.GetSpriteImage();

      im.Save(@"C:\Image.png");

      Console.WriteLine("Done....");
      Console.ReadLine();
    }
  }
}