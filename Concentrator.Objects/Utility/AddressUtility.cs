using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Concentrator.Objects.Utility.Address
{
  public static class AddressUtility
  {
    public static AddressNL FormatAddress(string address)
    {
      string housenumber = string.Empty;
      string housenumberExt = string.Empty;
      string street = string.Empty;
      int housenumberindex = address.Length;
      bool numberfilled = false;
      string lastitem = string.Empty;

      for (int i = address.Length; i > 0; i--)
      {
        string item = address.Substring(i - 1, 1);
        int number = 0;

        if (int.TryParse(item, out number))
        {
          if ((housenumberindex - i) > 1)
          {
            string tempstr = street;
            street = number.ToString() + street;
          }
          else
          {
            numberfilled = true;
            string tempnr = housenumber;
            housenumber = number.ToString() + tempnr;
            housenumberindex = i;
          }
        }
        else
        {
          if (numberfilled)
          {
            if (item.Contains("-"))
            {
              if (int.TryParse(lastitem, out number))
              {
                string tempnr = housenumber;
                housenumber = item + tempnr;
                housenumberindex = i;
              }
              else
              {
                string tempstr = street;
                street = item + street;
              }
            }
            else
            {
              string tempstr = street;
              street = item + street;
            }
          }
          else
          {
            string tempnrext = housenumberExt;
            housenumberExt = item + housenumberExt;
            housenumberindex = i;
          }
        }

        lastitem = item;
      }

      AddressNL result = new AddressNL();

      if (String.IsNullOrEmpty(street))
      {
        result.Street = address;
        result.IsValid = false;

      }
      else
      {
        street = street.Trim();

        if (street.Substring(street.Length - 1, 1).Contains("-"))
        {
          street = street.Substring(0, street.Length - 1);
        }


        result.Street = street;

        Regex hnRegex = new Regex(@"(.*?)([^\d].*)");
        Match m = hnRegex.Match(housenumber + housenumberExt);

        if (m.Groups.Count > 1)
        {
          result.HouseNumber = m.Groups[1].Value;
          result.HouseNumberExtension = m.Groups[2].Value;

        }
        else
        {
          result.HouseNumber = housenumber;
          result.HouseNumberExtension = String.Empty;

        }
        result.IsValid = true;
      }
      return result;
    }

    public static AddressNL FormatAddressUsingRegex(string address)
    {
      address = address.Trim();

      //((^[0-9-]+)? Check for possible number at the beginning(streetname) Group 1
      //[A-Za-z-\s.]+ Check for letters(streetname) Group 1
      //(\d[A-Za-z-\s]+)?) check for letters somewhere in the streetname (3e willem de "2e" straat) Group 1
      //get number(housenumber) Group 4
      //get possible extension Group 5
      var match = Regex.Match(address, @"((^[0-9-]+)?[A-Za-z-\s.]+(\d[A-Za-z\s]+)?)(\d+)([a-zA-Z0-9-\s]+)?", RegexOptions.IgnoreCase);
      var addressFinal = new AddressNL();
      if (match.Success)
      {
        addressFinal.Street = match.Groups[1].ToString();
        var s = match.Groups[0].ToString();
        var s2 = match.Groups[1].ToString();
        var s1 = match.Groups[2].ToString();
        addressFinal.HouseNumber = match.Groups[4].ToString();
        addressFinal.HouseNumberExtension += match.Groups[5].ToString();
        addressFinal.IsValid = true;

        return addressFinal;
      }

      return new AddressNL { IsValid = false };
    }

    public static string GetZipNumber(string zipcode)
    {
      Regex reg = new Regex("([0-9]{4,})");
      Match match = reg.Match(zipcode);
      if (match.Success)
        return match.Value;
      else
        return zipcode;
    }
  }


  public class AddressNL
  {
    public string Street { get; set; }
    public string HouseNumber { get; set; }
    public string HouseNumberExtension { get; set; }
    public bool IsValid { get; set; }
  }


}
