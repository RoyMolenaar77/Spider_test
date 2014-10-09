namespace Concentrator.Plugins.Wehkamp.Enums
{
  internal enum WehkampMessageStatus
  {
    ErrorDownload     = 0,  // communicator
    ValidationFailed  = 1,  // communicator
    MaxRetryExceeded  = 2,  // communicator
    Created           = 3,  // communicator -> plugins
    InProgress        = 4,  // plugins
    Success           = 5,  // plugins      -> communicator
    Error             = 6,  // plugins      -> communicator
    ErrorUpload       = 7,  // communicator
    Archived          = 8   // communicator
  }
}
