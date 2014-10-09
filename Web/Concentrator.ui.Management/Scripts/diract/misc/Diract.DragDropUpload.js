Diract.DragDropUpload = Ext.extend(Ext.Component, {
  constructor: function (config) {

    Diract.DragDropUpload.superclass.constructor.call(this, config);
    var id = config.ElementId;
    var self = this;
    var process = config.ProcessEl;
    if (!id) throw "no elementid(ElementId) specified";
    var settings = {
      filereader: typeof FileReader != 'undefined',
      dnd: 'draggable' in document.createElement('span'),
      formdata: !!window.FormData,
      progress: "upload" in new XMLHttpRequest
    },
        support = {
          filereader: document.getElementById('filereader'),
          formdata: document.getElementById('formdata'),
          progress: document.getElementById('progress')
        },
        acceptedTypes = config.acceptedTypes,

        progress = document.getElementById(process);

    this.waitMsg;
    this.inProgress = false;

    this.readfiles = function (files) {
      var isvalid = false;
 
      if (self.inProgress) return;
      self.inProgress = true;
      var formData = settings.formdata ? new FormData() : null;
      for (var i = 0; i < files.length; i++) {
        if (acceptedTypes) {
          for (var t = 0; t < acceptedTypes.length; t++) {
            if (files[i].type.indexOf(acceptedTypes[t]) !== -1) {
              isvalid = true;
              break;
            }
          }
        }
        if (isvalid || !acceptedTypes) {
          if (settings.formdata) formData.append('file', files[i]);
        }
      }

      if (isvalid || !acceptedTypes) {
        // now post a new XHR request
        if (settings.formdata) {
          self.waitMsg = Ext.MessageBox.wait(config.waitTitle || 'Please wait..', config.waitMsg || 'Uploading file');
          var xhr = new XMLHttpRequest();
          xhr.open('POST', config.Url);
          xhr.onload = self.beforeLoad.createDelegate(this);

          if (settings.progress) {
            xhr.upload.onprogress = function (event) {
         
            }
          }

          xhr.send(formData);
        }
      } else {
        
      }
    }
    
    this.setConfig = function(property, value){
      if (config[property])
        config[property] = value;
    }

    setTimeout(function () {
      var fileupload = document.getElementById(id);
      fileupload.addEventListener('dragover', self.onDragOver, false);
      fileupload.addEventListener('dragenter', self.onDragEnter, false);
      fileupload.addEventListener('dragleave', self.onDragLeave, false);
      fileupload.addEventListener('drop', self.onDrop.createDelegate(self), false);

    }, 1000);

  },
  onDrop: function (e) {
    var self = this;
    e.preventDefault();
   
    self.readfiles(e.dataTransfer.files);
  },
  onDragOver: function (e) {
    e.preventDefault();
    return false;
  },
  onDragEnter: function (e) {    
    e.preventDefault();
    return false;
  },
  onDragLeave: function (e) {
    e.preventDefault();
    return false;
  },
  beforeLoad: function(e){
    var self = this;
    self.waitMsg.hide();
    self.inProgress = false;
    this.onLoad(e);
  },
  onLoad: function (e) {

    //implement this function
    throw "Not yet implemented load function";
  }
});