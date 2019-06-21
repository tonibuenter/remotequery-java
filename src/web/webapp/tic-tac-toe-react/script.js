class Square extends React.Component {
  // TODO: remove the constructor
  constructor(props) {
    super(props);
    this.state = {
      value: null };

  }

  render() {
    // TODO: use onClick={this.props.onClick}
    // TODO: replace this.state.value with this.props.value
    return (
      React.createElement("button", { className: "square", onClick: () => this.setState({ value: 'X' }) },
      this.state.value));


  }}


class Board extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      squares: Array(9).fill(null) };

  }

  renderSquare(i) {
    return React.createElement(Square, { value: this.state.squares[i] });
  }

  render() {
    const status = 'Next player: X';

    return (
      React.createElement("div", null,
      React.createElement("div", { className: "status" }, status),
      React.createElement("div", { className: "board-row" },
      this.renderSquare(0), this.renderSquare(1), this.renderSquare(2)),

      React.createElement("div", { className: "board-row" },
      this.renderSquare(3), this.renderSquare(4), this.renderSquare(5)),

      React.createElement("div", { className: "board-row" },
      this.renderSquare(6), this.renderSquare(7), this.renderSquare(8))));



  }}


class Game extends React.Component {
  render() {
    return (
      React.createElement("div", { className: "game" },
      React.createElement("div", { className: "game-board" },
      React.createElement(Board, null)),

      React.createElement("div", { className: "game-info" },
      React.createElement("div", null),
      React.createElement("ol", null))));



  }}


// ========================================

ReactDOM.render(
React.createElement(Game, null),
document.getElementById('root'));