using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento
{
  public class Slugger
  {
    private static Dictionary<char, string> _translator = new Dictionary<char, string>();
    static Slugger()
    {
      _translator.Add('Š', "S"); _translator.Add('š', "s"); _translator.Add('Ð', "Dj");
      _translator.Add('Ž', "Z"); _translator.Add('ž', "z"); _translator.Add('À', "A"); _translator.Add('Á', "A");
      _translator.Add('Â', "A"); _translator.Add('Ã', "A"); _translator.Add('Ä', "A"); _translator.Add(
    'Å', "A"); _translator.Add('Æ', "A"); _translator.Add('Ç', "C"); _translator.Add('È', "E"); _translator.Add('É', "E");
      _translator.Add('Ê', "E"); _translator.Add('Ë', "E"); _translator.Add('Ì', "I"); _translator.Add('Í', "I"); _translator.Add('Î', "I"); _translator.Add(
    'Ï', "I"); _translator.Add('Ñ', "N"); _translator.Add('Ò', "O"); _translator.Add('Ó', "O"); _translator.Add('Ô', "O");
      _translator.Add('Õ', "O"); _translator.Add('Ö', "O"); _translator.Add('Ø', "O"); _translator.Add('Ù', "U"); _translator.Add('Ú', "U"); _translator.Add(
    'Û', "U"); _translator.Add('Ü', "U"); _translator.Add('Ý', "Y"); _translator.Add('Þ', "B"); _translator.Add('ß', "Ss");
      _translator.Add('à', "a"); _translator.Add('á', "a"); _translator.Add('â', "a"); _translator.Add('ã', "a"); _translator.Add('ä', "a"); _translator.Add(
     'å', "a"); _translator.Add('æ', "a"); _translator.Add('ç', "c"); _translator.Add('è', "e"); _translator.Add('é', "e");
      _translator.Add('ê', "e"); _translator.Add('ë', "e"); _translator.Add('ì', "i"); _translator.Add('í', "i"); _translator.Add('î', "i"); _translator.Add(
    'ï', "i"); _translator.Add('ð', "o"); _translator.Add('ñ', "n"); _translator.Add('ò', "o"); _translator.Add('ó', "o");
      _translator.Add('ô', "o"); _translator.Add('õ', "o"); _translator.Add('ö', "o"); _translator.Add('ø', "o");
      _translator.Add('ù', "u"); _translator.Add(
    'ú', "u"); _translator.Add('û', "u"); _translator.Add('ý', "y"); _translator.Add('þ', "b");
      _translator.Add('ÿ', "y"); _translator.Add('ƒ', "f");


    }

    public static string StripAccents(string input)
    {
      return input;
    }


    public static object Slug(string input, bool allowSlashes = false)
    {
      input = StripAccents(input.Trim().ToLower());

      return input;




    }
  }
}
