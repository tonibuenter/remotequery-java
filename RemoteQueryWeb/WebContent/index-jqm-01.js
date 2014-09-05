$(document).ready(function() {

  var page$, header$, content$, footer$, _loginUi;

  page$ = $('<div>', {
    'data-role' : 'page'
  }).append(
  // header
  header$ = $('<div>', {
    'data-role' : 'header'
  }),
  // content
  content$ = $('<div>', {
    'data-role' : 'content'
  }),
  // footer
  footer$ = $('<div>', {
    'data-role' : 'footer'
  }));
  init(page$, header$, content$, footer$);

  // append and show page
  $('body').append(page$);

  $("body").pagecontainer("change", page$, {
    transition : 'flip'
  });

  function init(page$, header$, content$, footer$) {

    //
    // header
    //
    header$.empty().append($('<h1>Hello</h1>'));
    //
    // header
    //
    content$.empty().append(APP_ui.button('show all', function() {
      alert('hello !!!!!!!!!!!!!!');
    }));
    //
    // footer
    //
    footer$.empty().append(APP_ui.button('bye bye', function() {
      alert('bye');
    }));

    //
    _loginUi = loginUi();
    content$.append(_loginUi.view());

    //

    $('body').enhanceWithin();
  }

  function loginUi() {
    var view$ = $('<div>');
    var ui = APP_ui.templateUi({
      'view$' : view$
    });

    init();
    return ui;

    function init() {
      var t1 = APP_base.newId(), t2 = APP_base.newId();
      var userId$, secretWord$;
      view$.append($('<div>').append($('<label>', {
        'for' : t1,
        'text' : 'User Id'
      }), userId$ = $('<input>', {
        'type' : 'text',
        'id' : t1
      }), $('<label>', {
        'for' : t2,
        'text' : 'Password'
      }), secretWord$ = $('<input>', {
        'type' : 'password',
        'id' : t2
      }), APP_ui.button('login', function() {
        rQ.call('Login', {
          'userId' : userId$.val(),
          'secretWord' : secretWord$
        }, function(data) {
          var list = rQ.toList(data);

        });
      })));

      // <form>
      // <label for="text-1">Text input:</label>
      // <input type="text" name="text-1" id="text-1" value="">
      // <label for="text-3">Text input: data-clear-btn="true"</label>
      // <input type="text" data-clear-btn="true" name="text-3" id="text-3"
      // value="">
      // </form>
    }
    
    


    function loginUi(serviceId) {
      var ui, view$, userId$, passwd$, login$, clear$, id;

      view$ = $('<div>').addClass('ui-field-contain');
      id = 'login-' + APP_base.newId();
      userId$ = $('<input>', {
        'type' : 'text',
        'id' : id
      });
      view$.append($('<label>', {
        'for' : id,
        'text' : 'User Id'
      }), userId$);
      id = 'login-' + APP_base.newId();
      passwd$ = $('<input>', {
        'type' : 'password',
        'id' : id
      });
      view$.append($('<label>', {
        'for' : id,
        'text' : 'Password'
      }), passwd$);
      login$ = APP_ui.button('Login', function() {
        var s = userId$.val();
        APP_data.callSq(serviceId, {
          'userId' : s,
          'secretWord' : passwd$.val()
        }, function() {
          alert('login result');
        });
      }).addClass('ui-btn');
      clear$ = APP_ui.button('Clear', function() {
        userId$.val('');
        passwd$.val('');
      }).addClass('ui-btn');
      view$.append($('<div>').append(login$, clear$));
      ui = APP_ui.templateUi({
        'view$' : view$
      });

      return ui;

    }


  }

});
