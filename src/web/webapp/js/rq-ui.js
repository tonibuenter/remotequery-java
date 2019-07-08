var rQ = rQ || {};

(function() {

  //
  // ELEMENT FUN
  //
  function elementFun(ele, firstClass) {
    return function(arg0, arg1, children) {
      var ele$;

      if (typeof arg0 == 'object') {
        ele$ = $('<' + ele + '>', arg0);
      } else {
        ele$ = $('<' + ele + '>');
      }

      if (!_.isObject(arg0) && !_.isUndefined(arg0)) {
        if (_.isUndefined(arg1) && firstClass) {
          ele$.addClass(arg0);
        } else {
          ele$.text(arg0);
        }
      }

      if (!_.isObject(arg1) && !_.isUndefined(arg1)) {
        ele$.addClass(arg1);
      }
      return ele$;
    }
  }
  // ELEMENT FUN -end-

  rQ.div = elementFun('div', true);

  rQ.p = elementFun('p');
  rQ.span = elementFun('span');
  rQ.cite = elementFun('cite');
  rQ.pre = elementFun('pre');
  rQ.code = elementFun('code');
  rQ.i = elementFun('i');
  rQ.a = elementFun('a');

  rQ.ul = elementFun('ul', true);
  rQ.ol = elementFun('ol', true);
  rQ.li = elementFun('li');

  rQ.img = elementFun('img', true);

  rQ.h1 = elementFun('h1');
  rQ.h2 = elementFun('h2');
  rQ.h3 = elementFun('h3');
  rQ.h4 = elementFun('h4');
  rQ.h5 = elementFun('h5');
  rQ.h6 = elementFun('h6');

  rQ.table = elementFun('table', true);
  rQ.thead = elementFun('thead', true);
  rQ.tbody = elementFun('tbody', true);
  rQ.tr = elementFun('tr', true);
  rQ.td = elementFun('td');

  rQ.form = elementFun('form', true);
  rQ.input = elementFun('input', true);
  rQ.label = elementFun('label');
  rQ.textarea = elementFun('textarea', true);
  rQ.select = elementFun('select', true);
  rQ.option = elementFun('option');

  rQ.section = elementFun('section', true);
  rQ.nav = elementFun('nav', true);

  //
  // UI
  //
  
  function templateUi(settings) {
    settings = settings || {};
    var ui, view$;
    var name, id, cx;
    var callbacks = {};

    name = settings.name;

    view$ = settings.view$ || settings.view || rQ.div();

    ui = {
      'id' : function(arg0) {
        if (_.isString(arg0)) {
          id = arg0;
          view$.attr('id', id);
          return ui;
        } else {
          return id;
        }
      },
      'view' : function(arg0) {
        if (_.isObject(arg0)) {
          view$ = arg0;
          return ui;
        }
        return view$;
      },
      'hide' : function() {
        view$.hide.apply(view$, arguments);
        return ui;
      },
      'show' : function() {
        view$.show.apply(view$, arguments);
        return ui;
      },
      'name' : function(arg) {
        if (_.isString(arg)) {
          name = arg;
          return ui;
        }
        return name;
      },
      'destroy' : function() {
        view$.remove();
        view$ = undefined;
      },
      'disable' : _.noop,
      'select' : function(arg0) {
        return handleCallback('select', arg0);
      },
      'change' : function(arg0) {
        return handleCallback('change', arg0);
      },
      'done' : function(arg0) {
        return handleCallback('done', arg0);
      },
      'action' : function(arg0) {
        return handleCallback('action', arg0);
      },
      'size' : function() {
        return view$.length;
      },
      'context' : function(arg0) {
        if (!_.isUndefined(arg0)) {
          cx = arg0;
        }
        return cx;
      },
      'editable' : function(arg0) {
        return rQ.handleDisabledAttr(view$, arg0);
      },
      'label' : _.noop,
      'value' : _.noop,
      'data' : _.noop
    };
    return ui;

    function handleCallback(name, fun) {
      if (_.isFunction(fun)) {
        callbacks[name] = fun;
      } else {
        if (_.isFunction(callbacks[name])) {
          callbacks[name].apply(this, arguments);
        }
      }
      return ui;
    }

  }
  rQ.ui = templateUi;
  rQ.templateUi = templateUi;
  
  function templateUi(arg0) {

    var ui, view$;
    var name, id, cx;
    var state = {};
    var callbacks = {};

    if (arg0 instanceof $) {
      view$ = arg0;
    } else {
      view$ = rQ.div();
    }

    ui = {
      'id' : function(arg0) {
        if (_.isString(arg0)) {
          id = arg0;
          view$.attr('id', id);
          return ui;
        } else {
          return id;
        }
      },
      'view' : function(arg0) {
        if (_.isObject(arg0)) {
          view$ = arg0;
          return ui;
        }
        return view$;
      },
      'hide' : function() {
        view$.hide.apply(view$, arguments);
        return ui;
      },
      'show' : function() {
        view$.show.apply(view$, arguments);
        return ui;
      },
      'name' : function(arg) {
        if (_.isString(arg)) {
          name = arg;
          return ui;
        }
        return name;
      },
      'destroy' : function() {
        view$.remove();
        view$ = undefined;
      },
      'disable' : _.noop,
      'select' : function(arg0) {
        return handleCallback('select', arg0);
      },
      'change' : function(arg0) {
        return handleCallback('change', arg0);
      },
      'done' : function(arg0) {
        return handleCallback('done', arg0);
      },
      'action' : function(arg0) {
        return handleCallback('action', arg0);
      },
      'size' : function() {
        return view$.length;
      },
      'context' : function(arg0) {
        if (arg0 == undefined) {
          cx = arg0;
        }
        return cx;
      },
      'editable' : function(arg0) {
        return rQ.handleDisabledAttr(view$, arg0);
      },
      'value' : function(args) {
        if (args != undefined) {
          view$.val(args);
        }
        return view$.val();
      },
      'data' : _.noop
    };
    return ui;

    function handleCallback(name, fun) {
      if (_.isFunction(fun)) {
        callbacks[name] = fun;
      } else {
        if (_.isFunction(callbacks[name])) {
          callbacks[name].apply(this, arguments);
        }
      }
      return ui;
    }

  }
  rQ.ui = templateUi;

  // TEMPLATE UI -end-

  //
  // Y
  //
  function y(componentOrTag, properties, children) {
    var r;
    if (typeof componentOrTag == 'string') {
      r = $('<' + componentOrTag + '>', properties);
    } else if (typeof componentOrTag == 'function') {
      r = componentOrTag(properties);
    }
    if (r instanceof $) {
      if (typeof children == 'string') {
        r.text(children);
      } else if (typeof children == 'object' && children.length) {
        for (var i = 0; i < children.length; i++) {
          r.append(h);
        }
      }
    }
  }
  rQ.y = y;

  //
  // INPUT UI
  //
  function inputUi(settings) {
    return rQ.ui(rQ.input(settings));
  }
  rQ.inputUi = inputUi;

  //
  // TEXTAREA UI
  //
  function textareaUi(settings) {
    return rQ.ui(rQ.textarea(settings));
  }
  rQ.textareaUi = textareaUi;

  rQ.button = function(label, callback) {
    return $('<button>').text(label).on('click', callback);
  }

  //
  // BUTTON UI
  //
  function buttonUi(label, cb) {
    var ui, a$, disabled;

    a$ = rQ.a().attr('href', '#').text(label).click(_action);
    a$.addClass('waves-effect waves-green btn');
    ui = rQ.ui({
      'view' : a$
    });

    cb = cb || _.noop;
    ui.disable = disable;
    return ui;

    function _action() {
      if (!disabled) {
        cb.apply(this, arguments);
      }
    }

    function disable(arg0) {
      if (_.isUndefined(arg0)) {
        return disabled;
      }
      if (arg0) {
        disabled = true;
        a$.addClass('disabled');
      } else {
        disabled = false;
        a$.removeClass('disabled');
      }
      return ui;
    }
  }
  rQ.buttonUi = buttonUi;

  //
  // TINYMCE UI
  //
  function tinymceUi(settings) {

    settings = settings || {};

    var ui, view$, ed, id, currentValue;

    ui = rQ.ui();

    view$ = ui.view();

    id = 'tinymce' + rQ.newId();

    view$.attr('id', id);

    setTimeout(
        function() {
          var tmp;
          tmp = new tinymce.Editor(
              id,
              {
                // selector : '#myTextarea',
                theme : 'modern',
                // width : 600,
                height : 300,
                'plugins' : settings.plugins
                    || [
                        'advlist autolink link image lists charmap print preview hr anchor pagebreak spellchecker',
                        'searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking',
                        'save table contextmenu directionality emoticons template paste textcolor' ],
                // content_css : 'css/content.css',
                'toolbar' : settings.toolbar
                    || 'undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | print preview fullpage | forecolor backcolor emoticons'
              }, tinymce.EditorManager);

          tmp.render();
          ed = tmp;
          // ui.value(currentValue);
        }, 0);

    ui.value = function(arg) {
      if (arg) {
        currentValue = arg;
        view$.html(arg);
      } else {
        return currentValue = ed.getContent();
      }
    }

    return ui;

  }
  rQ.tinymceUi = tinymceUi;

  // TINYMCE UI

  //
  // TINYMCE EDITOR UI
  //
  /**
   * @memberOf rQ_ui_md
   */
  function tinymceEditorUi(settings) {

    var ui, view$, ed, idEditor, idToolbar, currentValue;
    var toolbar$, editor$;

    ui = rQ.ui();

    idToolbar = 'tinymceToolbar' + rQ.newId();
    idEditor = 'tinymce' + rQ.newId();

    toolbar$ = rQ.div('toolbar').attr('id', idToolbar);
    editor$ = rQ.div('editor').attr('id', idEditor);
    view$ = ui.view().addClass('tinymce-editor-ui').append(toolbar$, editor$);

    editor$.html(settings.htmlText);

    if (settings.viewOnly) {
      read();
    } else {

      setTimeout(
          function() {
            tinymce
                .init({

                  fixed_toolbar_container : '#' + idToolbar,
                  selector : '#' + idEditor,
                  inline : true,
                  plugins : [
                      'advlist autolink  link image lists charmap print preview hr anchor pagebreak spellchecker',
                      'searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking',
                      'save table contextmenu directionality emoticons template paste textcolor autosave' ],
                  toolbar : 'save undo redo lists searchreplace bullist numlist anchor preview',
                  visualblocks_default_state : false,
                  menubar : 'edit insert view format table',
                  skin : "lightgray",
                  statusbar : true,
                  menu : {

                    edit : {
                      title : 'Edit',
                      items : 'undo redo | cut copy paste pastetext | selectall'
                    },
                    insert : {
                      title : 'Insert',
                      items : 'link image |  hr'
                    },
                    view : {
                      title : 'View',
                      items : 'visualaid'
                    },
                    format : {
                      title : 'Format',
                      items : 'bold italic underline | bullist | numlist | strikethrough superscript subscript | formats | removeformat'
                    },
                    table : {
                      title : 'Table',
                      items : 'inserttable tableprops deletetable | cell row column'
                    }
                  },

                  selection_toolbar : 'bold italic | quicklink h2 h3 blockquote',

                  save_onsavecallback : function(s) {

                    save();

                  }
                });

            read();

          }, 0);
    }
    return ui;

    function save() {
      rQ.call(settings.saveServiceId, {
        'htmlTextId' : settings.htmlTextId,
        'htmlText' : editor$.html()
      }, function(data) {
        if (data.exception) {
          alert(data.exception);
        }
      });
    }

    function read() {
      rQ.call(settings.getServiceId, {
        'htmlTextId' : settings.htmlTextId
      }, function(data) {
        var e = rQ.toList(data)[0];
        if (data.exception) {
          alert(data.exception);
          return;
        }
        if (e) {
          editor$.html(e.htmlText);
        }
      });
    }

  }
  rQ.tinymceEditorUi = tinymceEditorUi;
  // TINYMCE EDITOR UI -end-

  //
  // TOAST
  //
  function toast(settings) {
    return rQ.div(settings).addClass('rq-toast');
  }
  rQ.toast = toast;

  // TOAST

})();