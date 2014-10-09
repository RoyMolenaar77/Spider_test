namespace Concentrator.Objects.Enumerations
{
  public enum RepositoryOperationStatus
  {
    /// <summary>
    /// The repository did not perform the operation it was intended to do.
    /// </summary>
    Nothing = 0,

    /// <summary>
    /// The repository successfully performed the operation and created the entity or entities.
    /// </summary>
    Created = 1,

    /// <summary>
    /// The repository successfully performed the operation and deleted the entity or entities.
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// The repository successfully performed the operation and updated the entity or entities.
    /// </summary>
    Updated = 3
  }
}
