$(document).ready(function() {

  var page$, header$, content$, footer$;
  // uis
  var main, login;
  // for login/session tracking
  var sessionId;
  // helper stuff
  var switchSupport = APP_ui.switchSupport();

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

    //
    // footer
    //

    //
    // content
    //

    main = mainUi();
    content$.append(main.view());
    switchSupport.put('main', main.view());

    login = loginUi("UserLogin");
    content$.append(login.view());
    switchSupport.put('login', login.view());
    login.done(function(arg0) {
      if (_.isString(arg0)) {
        sessionId = arg0;
        switchSupport.show('main');
      } else {
        // show error ...
      }
    });

    //

    $('body').enhanceWithin();

    switchSupport.show('login');

  }

  function mainUi() {
    var view$ = $('<div>');
    var ui = APP_ui.templateUi({
      'view$' : view$
    });

    view$.append($('<h2>').text('... main ui ...'));

    return ui;
  }

  function loginUi2() {
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
  }

  function loginUi(serviceId) {
    var ui, view$, userId$, passwd$, login$, clear$, id;
    //
    var doneCb;

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
      rQ.call(serviceId, {
        'userId' : userId$.val(),
        'secretWord' : passwd$.val()
      }, function() {
        alert('login result');
        // if session ok
        done('dummy');
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

    ui.done = done;
    return ui;

    function done(arg0) {
      if (_.isFunction(arg0)) {
        doneCb = arg0;
        return;
      }
      if (_.isFunction(doneCb)) {
        doneCb.apply(this, arguments);
      }
    }

  }

});
