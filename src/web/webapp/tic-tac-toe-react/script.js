var re = React.createElement;

function Square(props) {
  return (
    re("button", { className: "square", onClick: props.onClick },
    props.value));
}

class Board extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      squares: Array(9).fill(null),
      xIsNext: true };

  }

  handleClick(i) {
    const squares = this.state.squares.slice();
    squares[i] = this.state.xIsNext ? 'X' : 'O';
    this.setState({
      squares: squares,
      xIsNext: !this.state.xIsNext });

  }

  renderSquare(i) {
    return (
      re(Square, {
        value: this.state.squares[i],
        onClick: () => this.handleClick(i) }));


  }

  render() {
    const status = 'Next player: ' + (this.state.xIsNext ? 'X' : 'O');

    return (
      re("div", null,
      re("div", { className: "status" }, status),
      re("div", { className: "board-row" },
      this.renderSquare(0),
      this.renderSquare(1),
      this.renderSquare(2)),

      re("div", { className: "board-row" },
      this.renderSquare(3),
      this.renderSquare(4),
      this.renderSquare(5)),

      re("div", { className: "board-row" },
      this.renderSquare(6),
      this.renderSquare(7),
      this.renderSquare(8))));



  }}


class Game extends React.Component {
  render() {
    return (
      re("div", { className: "game" },
      re("div", { className: "game-board" },
      re(Board, null)),

      re("div", { className: "game-info" },
      re("div", null),
      re("ol", null))));



  }}


// ========================================

ReactDOM.render(
re(Game, null),
document.getElementById('root'));