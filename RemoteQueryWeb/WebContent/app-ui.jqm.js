var APP_ui = APP_ui || {};

//
// init function for alle APP_ui functions
//

(function() {
  var APP_ui_orig = APP_ui;

  // <button class="ui-btn">Button</button>
  function button() {
    var b$ = APP_ui_orig.button.apply(this, arguments);
    b$.addClass('ui-btn');
    return b$;
  }
})();
