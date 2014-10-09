namespace System.IO
{
  using Linq;
  using Security;
  using Security.AccessControl;
  using Security.Principal;

  public static class DirectoryInfoExtensions
  {
    /// <summary>
    /// Returns true if the current user has the specified access to directory.
    /// </summary>
    public static Boolean HasAccess(this DirectoryInfo directoryInfo, FileSystemRights flags)
    {
      if (directoryInfo == null)
      {
        throw new NullReferenceException("directoryInfo");
      }

      if (!directoryInfo.Exists)
      {
        throw new InvalidOperationException("directory does not exist");
      }

      return directoryInfo
        .GetAccessControl()
        .GetAccessRules(true, true, typeof(NTAccount))
        .OfType<FileSystemAccessRule>()
        .Any(rule => rule.AccessControlType == AccessControlType.Allow && rule.FileSystemRights.HasFlag(flags));
    }
  }
}
