namespace Concentrator.Objects.Bootstrapper.Installation
{
  /// <summary>
  /// Interface for an installer in the ConcentratorBootstrapper
  /// </summary>
  /// <remarks>Be aware! implementations of this will be autmatically ran at startup</remarks>
  public interface IInstaller
  {
    void Run();
  }
}