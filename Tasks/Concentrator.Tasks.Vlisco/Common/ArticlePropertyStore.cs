using System;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Common
{
  using Objects.Models.Attributes;

  public class ArticlePropertyStore
  {
    [ProductAttribute(Constants.Attribute.Collection)]
    public ProductAttributeMetaData Collection = null;

    [ProductAttribute(Constants.Attribute.ColorCode)]
    public ProductAttributeMetaData ColorCode = null;

    [ProductAttribute(Constants.Attribute.ColorName)]
    public ProductAttributeMetaData ColorName = null;

    [ProductAttribute(Constants.Attribute.LabelCode)]
    public ProductAttributeMetaData LabelCode = null;

    [ProductAttribute(Constants.Attribute.MaterialCode)]
    public ProductAttributeMetaData MaterialCode = null;

    [ProductAttribute(Constants.Attribute.OriginCode)]
    public ProductAttributeMetaData OriginCode = null;

    [ProductAttribute(Constants.Attribute.ReferenceCode)]
    public ProductAttributeMetaData ReferenceCode = null;

    [ProductAttribute(Constants.Attribute.ReplenishmentMaximum)]
    public ProductAttributeMetaData ReplenishmentMaximum = null;

    [ProductAttribute(Constants.Attribute.ReplenishmentMinimum)]
    public ProductAttributeMetaData ReplenishmentMinimum = null;

    [ProductAttribute(Constants.Attribute.ShapeCode)]
    public ProductAttributeMetaData ShapeCode = null;

    [ProductAttribute(Constants.Attribute.SupplierCode)]
    public ProductAttributeMetaData SupplierCode = null;

    [ProductAttribute(Constants.Attribute.SupplierName)]
    public ProductAttributeMetaData SupplierName = null;

    [ProductAttribute(Constants.Attribute.SizeCode)]
    public ProductAttributeMetaData SizeCode = null;

    [ProductAttribute(Constants.Attribute.StockType)]
    public ProductAttributeMetaData StockType = null;
  }
}
