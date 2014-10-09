using System;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal static class VendorItemHelper
  {
    /// <summary>
    /// Returns the Article Number from a vendor item number
    /// </summary>
    /// <param name="vendorItemNumber">Vendor Item Number</param>
    /// <returns>Article Number or empty string if not present</returns>
    internal static string GetArticleNumber(string vendorItemNumber)
    {
      var data = vendorItemNumber.Split(' ');
      if (data.Length < 1)
        return string.Empty;

      return data[0];
    }

    /// <summary>
    /// Returns the Color Number from a vendor item number
    /// </summary>
    /// <param name="vendorItemNumber">Vendor Item Number</param>
    /// <returns>Color Number or empty string if not present</returns>
    internal static string GetColorNumber(string vendorItemNumber)
    {
      var data = vendorItemNumber.Split(' ');

      if (data.Length < 2)
        return string.Empty;

      return data[1];
    }

    /// <summary>
    /// Returns the Display Size from a vendor item number
    /// </summary>
    /// <param name="vendorItemNumber">Vendor Item Number</param>
    /// <returns>Display Size or empty string if not present</returns>
    internal static string GetDisplaySize(string vendorItemNumber)
    {
      var data = vendorItemNumber.Split(' ');

      if (data.Length < 3)
        return string.Empty;

      if (data.Length > 3)
        return string.Format("{0} {1}", data[2], data[3]);

      return data[2];
    }

    /// <summary>
    /// Returns the gender from a vendor item number
    /// </summary>
    /// <param name="vendorItemNumber">Vendor Item Number</param>
    /// <param name="vendorId">Vendor ID</param>
    /// <returns>Gender or empty string if not present</returns>
    internal static string GetWehkampGender(string vendorItemNumber, int vendorId)
    {
      #region Coolcat Wehkamp (id 15)
      if (vendorId == 15)
      {
        switch (vendorItemNumber.Substring(0, 1))
        {
          case "1":
            return "Heren";

          case "2":
            return "Dames";

          case "3":
            return "Jongens";

          case "4":
            return "Meisjes";

          default:
            return string.Empty;
        }
      }
      #endregion

      #region America Today Wehkamp (id 25)
      if (vendorId == 25)
      {
        switch (vendorItemNumber.Substring(0, 1))
        {
          case "1":
            return "Heren";

          case "2":
            return "Dames";

          default:
            return string.Empty;
        }
      }
      #endregion

      throw new NotSupportedException(string.Format("Cannot convert gender. Vendor {0} not supported", vendorId));
    }

    /// <summary>
    /// Returns the sleeve length from a vendor item number and the product group
    /// </summary>
    /// <param name="vendorItemNumber">Vendor Item Number</param>
    
    /// <param name="vendorId">Vendor ID</param>
    /// <returns></returns>
    internal static string GetWehkampSleeveLength(string vendorItemNumber, int vendorId)
    {
      // Product Group Code (D,E,F,H,J,N) are supported; Second char in VendorItemNumber
      #region Coolcat Wehkamp (id 15)
      if (vendorId == 15)
      {
        switch (vendorItemNumber.Substring(1, 1).ToUpperInvariant())
        {
          case "D":
          case "E":
          case "F":
          case "H":
          case "J":
          case "N":
            {
              switch (vendorItemNumber.Substring(3, 1))
              {
                case "4":
                  return "Mouwloos";

                case "5":
                  return "Kort";

                case "6":
                  return "Lang";

                default:
                  return string.Empty;
              }
            }
          default:
            return string.Empty;
        }
      }
      #endregion

      #region America Today Wehkamp (id 25)
      if (vendorId == 25)
      {
        if (vendorItemNumber.Substring(1, 1) == "2")
        {
          switch (vendorItemNumber.Substring(2, 1))
          {
            case "6":
            case "8":
              return "Mouwloos";

            case "5":
            case "7":
              return "Kort";

            case "1":
            case "2":
            case "3":
            case "4":
              return "Lang";

            default:
              return string.Empty;
          }
        }

        return string.Empty;
      }
      #endregion

      throw new NotSupportedException(string.Format("Cannot convert sleeve length. Vendor {0} not supported", vendorId));
    }

  }
}
