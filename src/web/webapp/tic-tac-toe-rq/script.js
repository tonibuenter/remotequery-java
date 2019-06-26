//function Square(props) {
//  return (
//    re("button", { className: "square", onClick: props.onClick },
//    props.value));
//}

function squareUi(props) {

  return rQ.ui($('<button>', {
    'class' : 'square',
    'click' : props.onClick,
    'text' : props.value
  }));

}

// class Board extends React.Component {
// constructor(props) {
// super(props);
// this.state = {
// squares: Array(9).fill(null),
// xIsNext: true };
//
// }
//
// handleClick(i) {
// const squares = this.state.squares.slice();
// squares[i] = this.state.xIsNext ? 'X' : 'O';
// this.setState({
// squares: squares,
// xIsNext: !this.state.xIsNext });
//
// }
//
// renderSquare(i) {
// return (
// re(Square, {
// value: this.state.squares[i],
// onClick: () => this.handleClick(i) }));
//
//
// }
//
// render() {
// const status = 'Next player: ' + (this.state.xIsNext ? 'X' : 'O');
//
// return (
// re("div", null,
// re("div", { className: "status" }, status),
// re("div", { className: "board-row" },
// this.renderSquare(0),
// this.renderSquare(1),
// this.renderSquare(2)),
//
// re("div", { className: "board-row" },
// this.renderSquare(3),
// this.renderSquare(4),
// this.renderSquare(5)),
//
// re("div", { className: "board-row" },
// this.renderSquare(6),
// this.renderSquare(7),
// this.renderSquare(8))));
//
//
//
// }}

function boardUi() {
  var ui, view$, state;

  state = {
    squares : Array(9).fill(null),
    xIsNext : true
  };

  ui = rQ.ui(view$ = rQ.div());
  render();

  return ui;

  function handleClick(i) {
    var squares = state.squares.slice();
    squares[i] = state.xIsNext ? 'X' : 'O';
    state = {
      squares : squares,
      xIsNext : !state.xIsNext
    };
    render();
  }

  function renderSquare(i) {
    return squareUi({
      'value' : state.squares[i],
      'onClick' : () => handleClick(i)
    })
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

// class Game extends React.Component {
// render() {
// return (
// React.createElement("div", { className: "game" },
// React.createElement("div", { className: "game-board" },
// React.createElement(Board, null)),
//
// React.createElement("div", { className: "game-info" },
// React.createElement("div", null),
// React.createElement("ol", null))));
// }}

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

  // ReactDOM.render(
  // React.createElement(Game, null),
  // document.getElementById('root'));

  $('#game').append(gameUi().view());
});