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

})();
