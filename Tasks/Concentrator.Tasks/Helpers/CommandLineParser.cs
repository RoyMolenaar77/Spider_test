using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Concentrator.Tasks
{
  public class CommandLineParser
  {
    public String Command
    {
      get;
      set;
    }

    public ILookup<String, String> CommandParameters
    {
      get;
      private set;
    }

    public FileInfo Executable
    {
      get;
      private set;
    }

    public CommandLineParser(TextReader commandLineReader)
    {
      Parse(commandLineReader.ReadLine());
    }

    public CommandLineParser(String commandLine)
    {
      Parse(commandLine);
    }

    private static readonly Regex ExecutableRegex = new Regex("((?<QUOTE>^\")[^\"]*(?<-QUOTE>\")\\s+)(?(QUOTE)(?!))", RegexOptions.Compiled);

    private void Parse(String commandLine)
    {
      commandLine = commandLine.Trim();

      var executableMatch = ExecutableRegex.Match(commandLine);

      if (executableMatch.Success)
      {
      }
    }
  }
}
