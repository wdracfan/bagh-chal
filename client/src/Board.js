import {Component} from "react";
import {withRouter} from "react-router-dom";
import {Game} from "./Game";
import "./Board.css"

export class Board extends Component {
    constructor(props) {
        super(props);
        this.state = {
            userId: null,
            username: null,
            board: null,
            opponent: null,
            piece: null,
            currentTurn: null,
            rules: false
        }
    }
    
    return() {
        window.location.href = "/";
    }

    showRules() {
        this.setState({ rules: true });
    }
    closeRules() {
        this.setState({ rules: false });
    }
    
    render() {
        return <div>
            <button class="return-button" onClick={() => this.return()}>На главную</button>
            <button class="rules-button" onClick={() => this.showRules()}>Правила игры</button>
            <center><div class="center">
            <p><b>Вы:</b> {this.props.location.state.username}</p>
            {this.props.location.state.opponent != null && <p><b>Ваш соперник:</b> {this.props.location.state.opponent}</p>}
            {false && <p><b>Доска:</b> {this.props.location.state.board}</p>}
            <Game
                userId={this.props.location.state.userId} 
                boardId={this.props.location.state.board}
                piece={this.props.location.state.piece}
                currentTurn={this.props.location.state.currentTurn}
                withBot={this.props.location.state.opponent === null}
            />
            {this.state.rules &&
                <dialog open class="center rules-dialog">
                    <b>Правила игры</b><br/>
                    Козы ходят вот так<br/>
                    А тигры ходят вот так
                    <form method="dialog">
                        <button class="okay-button" onClick={() => this.closeRules()}>
                            Окей
                        </button>
                    </form>
                </dialog>}
            </div></center>
        </div>
    }
}

export const BoardWithRouter = withRouter(Board);