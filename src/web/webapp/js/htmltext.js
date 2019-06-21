(function() {

  $(init2);

  function init() {

    var tinymceEditorUi, editDiv$;

    editDiv$ = $('#editDiv');

    tinymceEditorUi = rQ.tinymceEditorUi({
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

    view$ = rQ.div('tinymce');
    toolbar$ = rQ.div('section');
    editor$ = rQ.div('section');
    view$.append(toolbar$, rQ.div('divider'), editor$);

    saveUi = rQ.buttonUi('save', saveHtmlText);
    toggle$ = rQ.button('...', toogleViewEdit);
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
          rQ.toast('No_Text_for ' + htmlTextId);
        }
      });
    }

    function saveHtmlText() {
      if (tinymceUi) {
        rQ.call('HtmlText.save', {
          'htmlTextId' : htmlTextId,
          'htmlText' : tinymceUi.value()
        }, function() {
          rQ.toast('Save_Done');
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
          editor$.empty().append(rQ.div('html-text-view').html(htmlText));
        });

      } else {
        saveUi.disable(false);
        toggle$.text('view');
        getHtmlText(function(htmlText) {
          tinymceUi = rQ.tinymceUi();
          editor$.empty().append(tinymceUi.view());
          tinymceUi.value(htmlText);
        });
        toggle$.text('view');
      }
    }
  }

})();