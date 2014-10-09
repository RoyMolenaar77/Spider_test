/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Diract.addComponent({ dependencies: ['Diract.ui.Grid'] }, function() {

  Diract.ui.TreeGrid = (function() {
    var tree = Ext.extend(Diract.ui.Grid, {
      /**
      Attach the cellclick listener. If the click is on the folder or arrow element next to name
      change object state
      */
      listeners: {
        'cellclick': function(grid, row, column, evt) {
          var a = new Ext.Element(evt.target);
          if (!a.hasClass(this.internalArrowClass) && !a.hasClass(this.internalFolderClass))
            return; //shortcircuit because the click is not on an item which enables expanding/collapsing

          //record the id of the clicked item and the parent id
          var id = grid.store.getAt(row).get(this.hierarchyKey);
          var parentID = grid.store.getAt(row).get(this.hierarchicalIDParentDataIndex);

          //record the cell element
          var cellEl = a.parent();

          this.toggle(id, cellEl, row, column, parentID);
        }
      },

      /**
      Initializes a renderer on the column enabled for the hierarchy  
      */
      initRenderer: function() {
        var column = null;
        var self = this;

        //Find the column based on the config option
        for (var i = 0; i < this.columnsSet.length; i++) {
          var col = this.columnsSet[i];
          if (col.dataIndex === this.hierarchicalColumn) {
            column = col;
            break;
          }
        }

        var origRenderer = column.renderer;

        //create a new renderer to include the extra visuals
        //if one was specified, call it as well
        var extendedRenderer = (function(val, m, rec) {
          var lvl = this.getLevelOfRecord(rec, 1);

          var arrowIconCls = this.internalArrowClass + ' ' + this.determineArrowStyle(rec.get(this.hierarchyKey), rec);
          var folderIconCls = this.internalFolderClass + ' ' + this.determineFolderStyle(rec.get(this.hierarchyKey), rec);

          var markup = '<img class="' + arrowIconCls + '" src="' + Ext.BLANK_IMAGE_URL + '" /><img class="' + folderIconCls + '" src="' + Ext.BLANK_IMAGE_URL + '"/> ';

          //add an extra empty image to have a white space
          for (i = 1; i < lvl; i++) {
            if (lvl != 1)
              markup = '<img src="' + Ext.BLANK_IMAGE_URL + '" class="ext-blank-image-url"/>' + markup;
          }

          if (origRenderer)
            markup = markup + origRenderer(val, m, rec);
          else
            markup = markup + val;

          return markup;
        }).createDelegate(self);

        column.renderer = extendedRenderer;
      },

      determineFolderStyle: function(val) {

        if (this.isIdInHierarchy(val, this.tracker)) {
          return this.folderClassExpanded;
        } else {
          return this.folderClassCollapsed;
        }
      },

      determineArrowStyle: function(val) {
        if (this.isIdInHierarchy(val, this.tracker)) {
          return this.arrowClassExpanded;
        } else {
          return this.arrowClassCollapsed;
        }
      },
      /**
      Get the level of an item based on its id
      */
      getLevel: function(id, level, trackerItem) {
        if (trackerItem.id == id) {
          return level;
        }
        if (trackerItem.child) {
          return this.getLevel(id, ++level, trackerItem.child);
        }
      },

      /**
      Returns the level of a certain element in the tree
      */
      getLevelOfRecord: function(rec, level) {
        var parent = rec.get(this.hierarchicalIDParentDataIndex);

        if (parent && parent != 0) {
          level++;
          //fetch parent record to continue recursion
          var parentRecord = this.store.getById(parent);

          if (parentRecord)
            level = this.getLevelOfRecord(parentRecord, level);
        }
        return level;
      },

      /**
      Ctor  
      */
      constructor: function(config) {
        Ext.apply(this, config);

        this.initClasses();
        this.initRowActions();

        Diract.ui.TreeGrid.superclass.constructor.call(this, config);

        this.initRenderer();
        this.initFields();

      },
      /**
      Initializes the fields used by the object - keys, grouping fields
      */
      initFields: function() {
        this.hierarchyKey = this.hierarchyKey || this.primaryKey;
      },


      /**
      Initializes the action responsible for adding  a new line 
      */
      initRowActions: function() {
        if (!this.rowActions)
          this.rowActions = [];
        this.rowActions.push(
          {
            text: 'Add item as a child',
            iconCls: 'add',
            handler: (function(r, e, t) {
              this.getNewLine(r);
            }).createDelegate(this)
          }
        )
      },

      /** 
      
      */
      getNewLine: function(r) {

        var that = this;
        this.showNewWindow(undefined,
        [{
          xtype: 'hidden',
          value: r.get(this.hierarchyKey),
          name: this.hierarchicalIDParentDataIndex
}],
        function() {
          var row = r.store.indexOfId(r.get(that.hierarchyKey)),
                              cellEl = new Ext.Element(that.view.getCell(row, 0)),
                              parentID = 0;

          var id = r.get(that.hierarchyKey);
          var filt = new Array();

          that.setTracker(id, cellEl, row, parentID);
          filt = that.createFilter(filt, id, that.tracker);

          that.expand(filt, id);
        });



        },

        /**  
        Tracking object of currently expanded (active) item
        */
        tracker: {
          id: null,
          cellEl: null,
          row: null,
          child: null,
          level: null
        },

        /**
        Resets tracker
        */
        resetTracker: function() {
          this.tracker.id = null;
          this.tracker.row = null;
          this.tracker.cellEl = null;
          this.tracker.child = null;
          this.tracker.level = null;
        },

        trackerSetter: function(id, cellEl, row, trackerItem) {
          trackerItem.id = id;
          trackerItem.row = row;
          trackerItem.cellEl = cellEl;
        },

        /**
        Convenience to set/update tracker obj
        */
        setTracker: function(id, cellEl, row, parentID) {
          //first get appropriate child

          //get the parent first
          var parent = this.getChildInTracker(this.tracker, parentID);

          if (!parent.id) parent = this.getChildInTracker(this.tracker, id); //in case parent is undefined

          if (parent.id === null) //no parent yet
          {
            this.trackerSetter(id, cellEl, row, parent);
          }

          else //there is a parent defined
          {
            if (parent.id == id) {
              this.trackerSetter(null, null, null, parent);
            }
            else if (parent.child) {
              if (parent.child.id != id) {
                parent.child = null;
                parent.child = {};
                this.trackerSetter(id, cellEl, row, parent.child);
              } else {
                this.trackerSetter(null, null, null, parent.child);
              }
            }
            else {
              parent.child = {};
              this.trackerSetter(id, cellEl, row, parent.child);
            }
          }
        },



        /**
        Recursive method to walk the hierarchy of the tracker. 
        Returns the child to which the new id will be attached
        Parameters: 
        @child - pass in the current child for the recursion
        @parentid - the parentid of the item to which' children the new child will be attached
        */
        getChildInTracker: function(item, id) {

          if (item.id === null || item.id === id) return item;

          else if (item.child)
            return this.getChildInTracker(item.child, id);

          else {
            item.child = {};
            return item.child;
          }
        },

        /**
        Gets an item from the hierarchy
        */
        getItemInTracker: function(item, id) {
          if (item == null) return item;

          if (item.id == id || item == null)
            return item;
          else
            return this.getItemInTracker(item.child, id);
        },

        /**
        Initialize all classes used by component
        */
        initClasses: function() {
          //unvolatile classes
          this.internalArrowClass = 'x-tree-arrow';
          this.internalFolderClass = 'x-tree-folder';

          this.arrowClassCollapsed = this.arrowClassCollapsed || 'x-tree-arrow-collapsed';
          this.arrowClassExpanded = this.arrowClassExpanded || 'x-tree-arrow-expanded';

          this.folderClassLoading = 'x-tree-folder-loading';
          this.folderClassCollapsed = this.folderClassCollapsed || 'x-tree-folder-collapsed';
          this.folderClassExpanded = this.folderClassExpanded || 'x-tree-folder-expanded';
        },

        /**
        Filters the current store to show/hide items
        Param : predicate - an expression returning bool to be executed on each item
        of the store when being filtered
        Predicate is optional. If not passed in the store 
        will only display items which are parents
        */
        filter: function(filterIDs) {

          var self = this;

          this.store.filterBy(function(record, id) {
            return filterIDs.indexOf(record.get(self.hierarchyKey)) != -1
                  || record.get(self.hierarchicalIDParentDataIndex) == 0
                  || filterIDs.indexOf(record.get(self.hierarchicalIDParentDataIndex)) != -1
          });


        },

        /**
        Toggle items  -- Params: 
        @id - ID of record (Grid primary key or other)
        */
        toggle: function(id, cellEl, row, column, parentID) {
          var isInHier = this.isIdInHierarchy(id, this.tracker);

          if (!isInHier && !this.isIdInHierarchy(parentID, this.tracker))
            this.resetTracker();

          var filt = new Array();

          this.setTracker(id, cellEl, row, parentID);
          filt = this.createFilter(filt, id, this.tracker);

          if (isInHier) { // we are just collapsing the item
            this.collapse(filt, id);
          }
          else {
            this.expand(filt, id);
          }
        },

        /**
        Checks to see if an id exists in the current hierarchy
        */
        isIdInHierarchy: function(id, item) {
          if (!item) return false;

          if (item.id == id) {
            return true;
          }
          else if (item.child != null) {
            return this.isIdInHierarchy(id, item.child);
          } else {
            return false;
          }
        },

        /**
        Creates the filter for the store on an expand/collapse event by walking the current hierarchy
      
      @filterIDArray - an array of ints to be displayed
        @id - the id of the item we are expanding
        @child in hierarchy
        */
        createFilter: function(filterIDArray, id, item) {
          if (item == null || !item.id) {
            return filterIDArray;
          }

          filterIDArray.push(item.id);

          if (id != item.id) {
            return this.createFilter(filterIDArray, id, item.child);
          }
          else { return filterIDArray };
        },

        /**
        Handles the expanding of the grid
        */
        expand: function(filt, id) {
          //first determine if records exist
          //if so filter on them
          //if not fetch them
          var self = this;
          var store = this.store;

          var el = this.getItemInTracker(this.tracker, id).cellEl;

          //get children of element
          var children = store.queryBy(
          function(record, recID) {
            if (record.get(this.hierarchicalIDParentDataIndex) == id)
              return true;
          }, self
        );

          if (children.getCount() == 0) {
            //load children
            this.setStateLoading(el);
            this.getChildren(id, el, function() { self.filter(filt); });
          }
          else {
            this.filter(filt);
          }
        },

        /**
        Handles collapsing of the grid
        */
        collapse: function(filt, id) {
          return this.filter(filt);
        },

        encodeHierarchy: function(parentItem, childitemid, ids) {
          ids.push(parentItem.id);

          if (parentItem.child && parentItem.id != childitemid)
            return this.encodeHierarchy(parentItem.child, childitemid, ids);

          return ids;
        },

        /**
        Gets children of an item
        */
        getChildren: function(itemId, el, action) {

          var self = this;
          Diract.silent_request({
            url: this.drilldownUrl,
            params: {
              parentID: itemId,
              ids: Ext.util.JSON.encode(this.encodeHierarchy(this.tracker, itemId, new Array()))
            },
            success: function(data) {
              self.addItems(data.results, itemId, el, action);
            }
          });
        },

        /** 
        Adds children items to the grid store
        */
        addItems: function(records, parentID, el, action) {
          var it = new Array();

          //var index = this.getItemInTracker(this.tracker, parentID).row + 1;
          var index = this.store.indexOfId(parentID) + 1;

          for (var i = 0; i < records.length; i++) {
            var record = records[i];
            var r = new this.store.recordType(record, record[this.hierarchyKey]);


            this.store.insert(index, r);

            Ext.each(this.primaryKey, function(key) {
              r.data[this.keyPrefix || '_' + key] = r.data[key];
            });

            index++;
          }

          this.setStateExpanded(el);
          if (action) {
            setTimeout(function() { action }, 1000);
          }
        },

        /**
        Set the current state to loading
        */
        setStateLoading: function(el) {
          this.setFolderClass(this.folderClassLoading, el);
        },

        /**
        Set the current state to collapsed
        */
        setStateCollapsed: function(el) {
          this.setFolderClass(this.folderClassCollapsed, el);
          this.setArrowClass(this.arrowClassCollapsed, el);
        },

        /**
        Set the current state to expanded
        */
        setStateExpanded: function(el) {
          this.setFolderClass(this.folderClassExpanded, el);
          this.setArrowClass(this.arrowClassExpanded, el);
        },

        /**
        A centralized setter for the folder class.       
        */
        setFolderClass: function(cl, el) {

          var folder = el.child('.' + this.internalFolderClass);

          //remove all class except internal one and add desired
          folder.removeClass(this.folderClassLoading);
          folder.removeClass(this.folderClassExpanded);
          folder.removeClass(this.folderClassCollapsed);

          folder.addClass(cl);
        },

        /**
        A centralized setter for the arrow class
        */
        setArrowClass: function(cl, el) {

          var arrow = el.child('.' + this.internalArrowClass);

          //remove all state classes and add desired one
          arrow.removeClass(this.arrowClassCollapsed);
          arrow.removeClass(this.arrowClassExpanded);
          arrow.addClass(cl);
        },

        /**
        Initializes the store
        */
        initStore: function() {
        }
      });
      /**
      Return the newly created instatnce
      */
      return tree;
    })();
  });