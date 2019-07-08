function squareUi(props) {

  return rQ.ui($('<button>', {
    'class' : 'square',
    'click' : props.onClick,
    'text' : props.value
  }));

}

function boardUi() {
  var ui, view$, state;

  state = {
    'squares' : Array(9).fill(null),
    'xIsNext' : true
  };

  ui = rQ.ui(view$ = rQ.div());
  render();

  return ui;

  function handleClick(i) {
    var squares = state.squares.slice();
    squares[i] = state.xIsNext ? 'X' : 'O';
    state = {
      'squares' : squares,
      'xIsNext' : !state.xIsNext
    };
    render();
  }

  function renderSquare(i) {
    return squareUi({
      'value' : state.squares[i],
      'onClick' : () => handleClick(i)
    });
  }

  function render() {
    var status, row$;
    status = 'Next player: ' + (state.xIsNext ? 'X' : 'O');
    view$.empty();
    view$.append(rQ.div(status, 'status'));
    _.each(state.squares, function(sq, i) {
      if (i % 3 == 0) {
        row$ = rQ.div('board-row').appendTo(view$)
      }
      row$.append(renderSquare(i).view());
    });
  }
}

function gameUi() {
  var ui;
  ui = rQ.ui();
  ui.view().append(rQ.div('game'));
  ui.view().append(rQ.div('game-board'));
  ui.view().append(boardUi().view().append());
  ui.view().append(rQ.div('game-info'), rQ.div(), rQ.ol());
  return ui;
}

// ========================================

$(function() {
  $('#game-ui').append(gameUi().view());
});