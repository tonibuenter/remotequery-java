var rQ = rQ || {};

(function() {



  //
  // INPUT UI -start-
  //
  /**
   * @memberOf rQ_ui_md
   */
  function inputUi(settings) {

    settings = settings || {};
    var ui, view$, input$, label$, icon$, id, isTextarea;
    //
    isTextarea = settings.isTextarea;
    //
    ui = rQ.ui();
    view$ = ui.view().addClass('input-field');
    input$ = isTextarea ? rQ.textarea('', 'materialize-textarea') : rQ
        .input();
    label$ = rQ.label();
    id = rQ.newId();
    label$.attr('for', id);
    input$.attr('id', id);
    input$.attr('type', settings.type || 'text').css('color', 'black');
    if (settings.name) {
      input$.attr('name', settings.name);
    }
    if (settings.active) {
      window.setTimeout(function() {
        input$.trigger('autoresize');
        label$.addClass('active');
      }, 0);
    }
    label$.text(settings.label).css('color', 'gray');
    if (settings.icon) {
      icon$ = rQ.i(settings.icon, 'material-icons prefix');
      view$.append(icon$);
    }
    view$.append(input$, label$);

    if (!isTextarea) {
      input$.keypress(function(e) {
        var code;
        if (e) {
          code = (e.keyCode ? e.keyCode : e.which);
        } else {
          code = 13;
        }
        if (code == 13) {
          e.preventDefault();
          ui.action(input$.text());
        }
        ui.change(input$.text());
      });
    }
    if (settings.css) {
      input$.css(settings.css);
    }

    ui.input$ = input$;
    ui.value = value;
    ui.editable = editable;

    return ui;

    function editable(is) {
      if (!is || is === 'false') {
        input$.attr('disabled', 'disabled');
        input$.css('color', 'gray');
      } else {
        input$.removeAttr('disabled');
        input$.css('color', 'black');
      }
    }

    function value(arg0) {
      if (!_.isUndefined(arg0)) {
        input$.val(arg0);
        window.setTimeout(function() {
          input$.trigger('autoresize');
          label$.addClass('active');
        }, 1);
      }
      return input$.val();
    }

  }
  rQ.inputUi = inputUi;

  // INPUT UI -end-

  //
  // TEXTAREA UI -start-
  //
  /**
   * @memberOf rQ_ui_md
   */
  function textareaUi(settings) {
    settings = settings || settings;
    settings.isTextarea = true;
    return inputUi(settings);
  }
  rQ.textareaUi = textareaUi;

  // TEXTAREA UI -end-

  //
  // BUTTON UI -start-
  //
  /**
   * @memberOf rQ_ui_md
   */
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

  // BUTTON UI -end-

  //
  // BUTTON -start-
  //
  /**
   * @memberOf rQ_ui_md
   */
  function button(label, cb) {
    var a$ = rQ.a().attr('href', '#').text(label).click(cb);
    a$.addClass('waves-effect waves-green btn');
    return a$;
  }
  rQ.button = button;

  // BUTTON -end-

  //
  // TINYMCE UI
  //
  /**
   * @memberOf rQ
   */
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

  // TINYMCE EDITOR UI

  //
  // TOAST
  //
  /**
   * @memberOf rQ_ui_md
   */
  function toast() {
    var s = rQ.span();
    Materialize.toast(s.append.apply(s, arguments), 5000, 'rounded');
  }
  rQ.toast = toast;

  // TOAST


})();