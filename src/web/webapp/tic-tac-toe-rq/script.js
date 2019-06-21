//class Square extends React.Component {
//  // TODO: remove the constructor
//  constructor(props) {
//    super(props);
//    this.state = {
//      value: null };
//
//  }
//
//  render() {
//    // TODO: use onClick={this.props.onClick}
//    // TODO: replace this.state.value with this.props.value
//    return (
//      React.createElement("button", { className: "square", onClick: () => this.setState({ value: 'X' }) },
//      this.state.value));
//
//
//  }}

function squareUi() {
  var state, ui, view$;

  state = {
    'value' : null
  };

  view$ = $('<button>', {
    'class' : 'square',
    'click' : function() {
      state.value = 'X';
      render();
    }
  });

  ui = rQ.ui(view$);

  return ui;

  function render() {
    ui.view().text(state.value);
  }
}

// class Board extends React.Component {
// constructor(props) {
// super(props);
// this.state = {
// squares: Array(9).fill(null) };
//
// }
//
// renderSquare(i) {
// return React.createElement(Square, { value: this.state.squares[i] });
// }
//
// render() {
// const status = 'Next player: X';
//
// return (
// React.createElement("div", null,
// React.createElement("div", { className: "status" }, status),
// React.createElement("div", { className: "board-row" },
// this.renderSquare(0), this.renderSquare(1), this.renderSquare(2)),
//
// React.createElement("div", { className: "board-row" },
// this.renderSquare(3), this.renderSquare(4), this.renderSquare(5)),
//
// React.createElement("div", { className: "board-row" },
// this.renderSquare(6), this.renderSquare(7), this.renderSquare(8))));
//
//
//
// }}

function boardUi() {
  var ui, view$, status, state;

  state = {
    'squares' : Array(9).fill(null)
  };

  ui = rQ.ui(view$ = rQ.div());
  init();

  return ui;

  function renderSquare(i) {
    var t = squareUi(state.squares[i]);
    return t;
  }

  function init() {
    var status, row$, t;
    status = 'Next player: X';
    view$.append(rQ.div(status, 'status'));

    _.each(state.squares, function(sq, i) {
      if (i % 3 == 0) {
        row$ = rQ.div('board-row').appendTo(view$)
      }
      t = renderSquare(i).view()
      row$.append(t);
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

  $('body').append(gameUi().view());
});