(function() {

  $(init2);

  function init() {

    var tinymceEditorUi, editDiv$;

    editDiv$ = $('#editDiv');

    tinymceEditorUi = rQ_ui.tinymceEditorUi({
      'htmlText' : editDiv$.text(),
      'htmlTextId' : editDiv$.attr('id'),
      'saveServiceId' : 'HtmlText.save',
      'getServiceId' : 'HtmlText.get'
    });

    editDiv$.empty().append(tinymceEditorUi.view());

  }

  function init2() {
    var editDiv$ = $('#editDiv');

    var htmlTextId = editDiv$.attr('id');
    var htmlText = editDiv$.text();

    tinymce(htmlTextId, htmlText).appendTo(editDiv$.empty());

  }

  function tinymce(htmlTextId, htmlText) {

    var view$, tinymceUi, toolbar$, editor$, saveUi, toggle$, isView;

    view$ = rQ_ui.div('tinymce');
    toolbar$ = rQ_ui.div('section');
    editor$ = rQ_ui.div('section');
    view$.append(toolbar$, rQ_ui.div('divider'), editor$);

    saveUi = rQ_ui.buttonUi('save', saveHtmlText);
    toggle$ = rQ_ui.button('...', toogleViewEdit);
    toolbar$.append(saveUi.view(), ' ', toggle$);

    toogleViewEdit();
    
    return view$;

    function getHtmlText(cb) {
      rQ.call('HtmlText.get', {
        'htmlTextId' : htmlTextId
      }, function(data) {
        var e = rQ.toList(data)[0];
        if (e) {
          cb(e.htmlText);
        } else {
          rQ_ui.toast('No_Text_for ' + htmlTextId);
        }
      });
    }

    function saveHtmlText() {
      if (tinymceUi) {
        rQ.call('HtmlText.save', {
          'htmlTextId' : htmlTextId,
          'htmlText' : tinymceUi.value()
        }, function() {
          rQ_ui.toast('Save_Done');
        });
      }
    }

    function toogleViewEdit() {

      tinymceUi = null;
      editor$.empty().text('loading ...');

      isView = !isView;

      if (isView) {
        saveUi.disable(true);
        toggle$.text('edit');
        getHtmlText(function(htmlText) {
          editor$.empty().append(rQ_ui.div('html-text-view').html(htmlText));
        });

      } else {
        saveUi.disable(false);
        toggle$.text('view');
        getHtmlText(function(htmlText) {
          tinymceUi = rQ_ui.tinymceUi();
          editor$.empty().append(tinymceUi.view());
          tinymceUi.value(htmlText);
        });
        toggle$.text('view');
      }
    }
  }

})();