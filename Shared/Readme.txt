EPPlus 2.6.0.1 

The component has been renamed EPPlus.dll, since I rewrote the last classes of the old Excelpackage. 
The interface is still the same though.

New features

* Named ranges
* Font-styling added to charts and shape-classes 
* TextRotation, locked and Hidden properties to Cell style
* Fixed InsertRow and DeleteRow
* Freeze/Unfreeze panes
* AutoFilter
* Sheet protection
* SaveAs stream and FileInfo
* Two new constructors to ExcelPackage (Output stream, Template Stream).
* Improved performance when saving.
* And a lot of small fixes.

--- Known Issues ---
* There was a bug in the ExcelColumn class, that set the Width of the column to 10 for accessed columns. 
  This has been removed so columns use default width until it has been set. This can make the position of 
  charts, shapes and picures to differ from version 2.5 behavior, if position is set with the SetPosition(PixelTop,PixelLeft) method.

See http://epplus.codeplex.com for more info