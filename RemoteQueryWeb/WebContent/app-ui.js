var APP_ui = APP_ui || {};

//
// init function for alle APP_ui functions
//

(function() {

})();

(function() {

  //
  // button
  //
  function button(arg0, arg1) {
    var b$ = $('<button>');
    if (_.isString(arg0)) {
      b$.text(arg0);
    }
    if (_.isFunction(arg0)) {
      b$.click(arg0);
    }
    if (_.isFunction(arg1)) {
      b$.click(arg1);
    }
    return b$;
  }
  APP_ui.button = button;

  //
  // input
  //
  function input() {
    var e$ = $('<input>');
    return e$;
  }
  APP_ui.input = input;

  /**
   * @memberOf APP_ui
   */
  function switchUi() {

    var panels = [], view$ = $('<div>').addClass('switch-panel-main'), previous, current;

    return {
      'view' : function() {
        return view$;
      },
      'put' : function(name, p$) {
        var i, e;
        view$.append(p$);
        for (i = 0; i < panels.length; i++) {
          e = panels[i];
          if (e.name === name) {
            e['panel$'].remove();
            e['panel$'] = p$;
            return this;
          }
        }
        panels.push({
          'name' : name,
          'panel$' : p$
        });
        return this;
      },
      'show' : function(name) {
        var i, e;
        for (i = 0; i < panels.length; i++) {
          e = panels[i];
          if (e.name === name) {
            e['panel$'].show();
            previous = current;
            current = name;
          } else {
            e['panel$'].hide();
          }
        }
      },
      'showPrevious' : function() {
        this.show(previous);
      }
    };

  } // end switchUi
  APP_ui.switchUi = switchUi;

  /**
   * @memberOf APP_ui
   */
  function switchSupport() {

    var vpanels = {}, previous = null, current = null;

    return {
      'put' : function(name, p$) {
        var vpanel = vpanels[name];
        if (vpanel === undefined) {
          vpanel = [];
          vpanels[name] = vpanel;
        }
        vpanel.push(p$);
        return p$;
      },
      'show' : function(name) {
        applyAll('hide');
        applyFun(name, 'show');
        previous = current;
        current = name;
      },
      'showOnly' : function(name) {
        applyAll('hide');
        applyFun(name, 'show');
        previous = current;
        current = name;
      },
      'applyAll' : applyAll,
      'apply' : function(name, methodName) {
        applyFun(name, methodName);
      },
      'showPrevious' : function() {
        this.show(previous);
      }
    };

    function applyAll(methodName) {
      $.each(vpanels, function(n, array) {
        applyFunSub(array, methodName);
      });
    }

    function applyFun(name, ifNameFunName, elseFunName) {
      var doElse = !_.isUndefined(elseFunName);
      $.each(vpanels, function(n, array) {
        if (n === name) {
          applyFunSub(array, ifNameFunName);
        } else if (doElse) {
          applyFunSub(array, elseFunName);
        }
      });
    }
    function applyFunSub(vpanel, funName) {
      var i, e$;
      for (i = 0; i < vpanel.length; i++) {
        e$ = vpanel[i];
        if (_.isFunction(e$[funName])) {
          e$[funName]();
        }
      }
    }
  } // end switchUi
  APP_ui.switchSupport = switchSupport;

  /**
   * @memberOf APP_ui
   * 
   */
  function templateUi(settings) {
    settings = settings || {};
    var view$ = $('<div>');
    var views = [ view$ ];
    var name, id;
    name = settings.name;
    // view$ provided ...
    if (_.isObject(settings.view$)) {
      view$ = settings.view$;
      views = [ view$ ];
    }
    if (_.isArray(settings.views) && settings.views.length > 0) {
      views = settings.views;
      view$ = views[0];
    }
    return {
      'id' : function(arg0) {
        if (_.isUndefined(arg0)) {
          return id;
        }
        id = arg0;
        if (_.isObject(view$) && _.isFunction(view$.attr)) {
          view$.attr('id', id);
        }
        return this;
      },
      'view' : function(arg0) {
        if (_.isObject(arg0)) {
          view$ = arg0;
          views = [];
        }
        return view$;
      },
      'views' : function(arg0) {
        if (_.isArray(arg0)) {
          views = arg0;
        }
        return views;
      },
      'hide' : function() {
        view$.hide.apply(view$, arguments);
        $.each(views, function(i, v$) {
          v$.hide.apply(v$, arguments);
        });
      },
      'show' : function() {
        view$.show.apply(view$, arguments);
        $.each(views, function(i, v$) {
          v$.show.apply(v$, arguments);
        });

      },
      'name' : function(arg) {
        if (_.isString(arg)) {
          name = arg;
        }
        return name;
      },
      'value' : function(arg) {
        if (_.isFunction(view$.val)) {
          if (!_.isUndefined(arg)) {
            view$.val(arg);
          } else {
            return view$.val();
          }
        }
      },
      'destroy' : function() {
        view$.remove();
        view$ = undefined;
        $.each(views, function(i, v$) {
          v$.remove();
        });
        views = undefined;
      },
      'disable' : function(arg0) {
        if (_.isBoolean(arg0)) {
          view$.prop('disabled', arg0);
        }
        return view$.prop('disabled');
      }
    };
  }
  APP_ui.templateUi = templateUi;

})();
