$(document).ready(function() {

  var page$, header$, content$, footer$;

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

    $('body').enhanceWithin();
  }
});
