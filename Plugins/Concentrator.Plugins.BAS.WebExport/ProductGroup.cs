using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Concentrator.Plugins.BAS.WebExport
{
  public partial class ProductGroup
  {
    private EntityRef<ProductGroup> _parentProductGroup;

    private EntitySet<ProductGroup> _childProductGroups;

    partial void OnCreated()
    {
      _parentProductGroup = default(EntityRef<ProductGroup>);
      _childProductGroups = new EntitySet<ProductGroup>(c => c.ParentProductGroup = this, c => c.ParentProductGroup = null);
    }

    public string GetProductGroupCodeTree()
    {
      List<string> result = new List<string>();

      result.Add(this.BackendProductGroupCode);

      var parent = this.ParentProductGroup;

      while (parent != null)
      {

        result.Add(parent.BackendProductGroupCode);

        parent = parent.ParentProductGroup;

      }
      return String.Join("/", result.ToArray());
    }

    [Association(Name = "FK_ProductGroup_ProductGroup", Storage = "_childProductGroups", ThisKey = "ParentProductGroupID", OtherKey = "ProductGroupID", DeleteRule = "NO ACTION")]
    public EntitySet<ProductGroup> ChildProductGroups
    {

      get { return _childProductGroups; }

      set { _childProductGroups.Assign(value); }

    }

    [Association(Name = "FK_ProductGroup_ProductGroup", Storage = "_parentProductGroup", OtherKey = "ProductGroupID", ThisKey = "ParentProductGroupID", IsForeignKey = true)]
    public ProductGroup ParentProductGroup
    {

      get { return _parentProductGroup.Entity; }

      set
      {

        ProductGroup previousValue = _parentProductGroup.Entity;

        if (((previousValue != value)

             || (_parentProductGroup.HasLoadedOrAssignedValue == false)))
        {

          SendPropertyChanging();

          if ((previousValue != null))
          {

            _parentProductGroup.Entity = null;

            previousValue.ChildProductGroups.Remove(this);

          }

          _parentProductGroup.Entity = value;

          if ((value != null))
          {

            value.ChildProductGroups.Add(this);

            _ParentProductGroupID = value.ProductGroupID;

          }

          else
          {

            _ParentProductGroupID = default(int);

          }

          SendPropertyChanged("ParentProductGroup");

        }

      }

    }
  }
}
