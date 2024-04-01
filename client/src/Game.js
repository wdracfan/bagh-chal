import {Component} from "react";
import hubConnection from "./WebSocket";
import "./Game.css"
import goatImage from "./goat.jpg"
import tigerImage from "./tiger.jpg"
import {HubConnectionState} from "@microsoft/signalr";

export class Game extends Component {
    constructor(props) {
        super(props);
        this.state = {
            userId: props.userId,
            boardId: props.boardId,
            pieces: [['Ti', 'O', 'O', 'O', 'Ti'],
                     ['O', 'O', 'O', 'O', 'O'],
                     ['O', 'O', 'O', 'O', 'O'],
                     ['O', 'O', 'O', 'O', 'O'],
                     ['Ti', 'O', 'O', 'O', 'Ti']],
            piece: props.piece,
            boardGoats: 0,
            spareGoats: 20,
            currentTurn: props.currentTurn,
            exRow: null,
            exCol: null,
            winner: null,
            withBot: props.withBot,
            rules: false
        }
    }
    
    // TODO: научиться определять конец игры (в api) + в api всё же использовать зачем-нибудь базу
    
    isCorrectMove(exRow, exCol, newRow, newCol, checkEmpty = true) {
        if (checkEmpty) {
            if (this.state.pieces[newRow][newCol] !== 'O') {
                return false;
            }
        }
        if (exRow === newRow && exCol === newCol) {
            return false;
        }
        if (exRow % 2 === exCol % 2 
            && Math.abs(exRow - newRow) <= 1 
            && Math.abs(exCol - newCol) <= 1) {
            return true;
        }
        if (exRow % 2 !== exCol % 2
            && Math.abs(exRow - newRow) === 0
            && Math.abs(exCol - newCol) === 1) {
            return true;
        }
        if (exRow % 2 !== exCol % 2
            && Math.abs(exRow - newRow) === 1
            && Math.abs(exCol - newCol) === 0) {
            return true;
        }
        return false;
    }
    
    isCorrectTakeMove(exRow, exCol, newRow, newCol) {
        if (this.state.pieces[newRow][newCol] !== 'O') {
            console.log("here A");
            return false;
        }
        // ход не туда
        if (Math.abs(exRow - newRow) > 2 || Math.abs(exCol - newCol) > 2) {
            console.log("here B");
            return false;
        }
        // не съедающий ход
        if (Math.abs(exRow - newRow) < 2 && Math.abs(exCol - newCol) < 2) {
            console.log("here C");
            return false;
        }
        // проверяем, что ход:
        // 1) не буквой Г
        // 2) через козу
        // 3) по линиям
        if ((exRow + newRow) % 2 === 0 
            && (exCol + newCol) % 2 === 0 
            && this.isCorrectMove(exRow, exCol, Math.floor((exRow + newRow) / 2), Math.floor((exCol + newCol) / 2), false) 
            && this.state.pieces[Math.floor((exRow + newRow) / 2)][Math.floor((exCol + newCol) / 2)] === 'G') {
            return true;
        } else {
            console.log((exRow + newRow) % 2 === 0);
            console.log((exCol + newCol) % 2 === 0);
            console.log(this.isCorrectMove(exRow, exCol, Math.floor((exRow + newRow) / 2), Math.floor((exCol + newCol) / 2)), false);
            console.log(this.state.pieces[Math.floor((exRow + newRow) / 2)][Math.floor((exCol + newCol) / 2)] === 'G');
            return false;
        }
    }
    
    async onClick(row, col) {
        if (this.state.currentTurn) {
            if (this.state.piece === "goat") {
                // поставить новую козу на пустое место
                if (this.state.spareGoats > 0 && this.state.pieces[row][col] === 'O') {
                    await hubConnection.invoke("PlaceNewGoat", this.state.boardId, row, col);
                    await hubConnection.invoke("ChangeTurn", this.state.boardId, this.state.withBot, this.state.piece);
                } else if (this.state.spareGoats === 0) {
                    // переместить козу
                    if (this.state.exRow === null && this.state.pieces[row][col] === 'G') {
                        this.setState({
                            exRow: row,
                            exCol: col,
                        });
                    } else if (this.state.exRow !== null
                        && this.isCorrectMove(this.state.exRow, this.state.exCol, row, col)) {
                        await hubConnection.invoke("MoveGoat", this.state.boardId, this.state.exRow, this.state.exCol, row, col);
                        this.setState({
                            exRow: null,
                            exCol: null
                        });
                        await hubConnection.invoke("ChangeTurn", this.state.boardId, this.state.withBot, this.state.piece);
                    } else {
                        this.setState({
                            exRow: null,
                            exCol: null
                        });
                    }
                }
            }
            if (this.state.piece === "tiger") {
                if (this.state.exRow === null && this.state.pieces[row][col] === 'Ti') {
                    // выбрать тигра для перемещения
                    console.log("here 1");
                    this.setState({
                        exRow: row,
                        exCol: col,
                    });
                } else if (this.state.exRow !== null 
                    && (this.isCorrectMove(this.state.exRow, this.state.exCol, row, col) 
                        || this.isCorrectTakeMove(this.state.exRow, this.state.exCol, row, col))) {
                    // переместить тигра
                    console.log("here 2");
                    if (this.isCorrectTakeMove(this.state.exRow, this.state.exCol, row, col)) {
                        await hubConnection.invoke(
                            "TakeGoat", 
                            this.state.boardId,
                            Math.floor((this.state.exRow + row) / 2),
                            Math.floor((this.state.exCol + col) / 2));
                    }
                    await hubConnection.invoke("MoveTiger", this.state.boardId, this.state.exRow, this.state.exCol, row, col);
                    this.setState({
                        exRow: null,
                        exCol: null
                    });
                    await hubConnection.invoke("ChangeTurn", this.state.boardId, this.state.withBot, this.state.piece);
                } else {
                    // отменить выбор тигра, если ход неправильный
                    this.setState({
                        exRow: null,
                        exCol: null
                    });
                }
            }
        }
    }
    
    async componentDidMount() {
        // перекидывает на главную страницу, если hub connection не установлено
        if (hubConnection.state !== HubConnectionState.Connected) {
            window.location.href = "/";
        }

        await hubConnection.on("place_new_goat_response", (m1, m2) => {
            this.setState(function(prevState, props) {
                let arr = prevState.pieces;
                console.log(arr);
                console.log(m1, m2);
                arr[m1][m2] = 'G';
                return {
                    pieces: arr,
                    spareGoats: prevState.spareGoats - 1,
                    boardGoats: prevState.boardGoats + 1
                };
            })
        });
        await hubConnection.on("change_turn_response", () => {
            this.setState(function(prevState, props) {
                return {
                    currentTurn: !(prevState.currentTurn)
                }
            })
        });
        await hubConnection.on("move_tiger_response", (exRow, exCol, newRow, newCol) => {
            console.log("move tiger", exRow, exCol, newRow, newCol);
            this.setState(function(prevState, props) {
                let arr = prevState.pieces;
                arr[exRow][exCol] = 'O';
                arr[newRow][newCol] = 'Ti';
                return {
                    pieces: arr
                };
            })
        });
        await hubConnection.on("move_goat_response", (exRow, exCol, newRow, newCol) => {
            this.setState(function(prevState, props) {
                let arr = prevState.pieces;
                arr[exRow][exCol] = 'O';
                arr[newRow][newCol] = 'G';
                return {
                    pieces: arr
                };
            })
        });
        await hubConnection.on("take_goat_response", (row, col) => {
            console.log("take goat", row, col);
            this.setState(function(prevState, props) {
                let arr = prevState.pieces;
                arr[row][col] = 'O';
                return {
                    pieces: arr,
                    boardGoats: prevState.boardGoats - 1
                };
            })
        });
        await hubConnection.on("check_winner_response", (winner) => {
            console.log("WINNER", winner);
            this.setState({
                winner: winner
            })
        });
        if (this.state.withBot && !this.state.currentTurn) {
            console.log("sfsfsf");
            await hubConnection.invoke("ChangeTurn", this.state.boardId, this.state.withBot, this.state.piece);
            this.setState({
                currentTurn: true
            })
        }
    }
    
    getImage(piece) {
        if (piece === "tiger") {
            return tigerImage
        } else {
            return goatImage
        }
    }
    
    getOpponentImage(piece) {
        if (piece === "tiger") {
            return goatImage
        } else {
            return tigerImage
        }
    }
    
    getMoveImage(isCurrentMove) {
        if (isCurrentMove) {
            return this.getImage(this.state.piece)
        } else {
            return this.getOpponentImage(this.state.piece)
        }
    }
    
    getStyle(piece) {
        if (piece === 'Ti') {
            return {
                display: `inline-block`,
                width: `50px`,
                height: `50px`,
                backgroundImage: `url(${tigerImage})`,
                backgroundSize: `50px 50px`
            }
        } else if (piece === 'G') {
            return {
                display: `inline-block`,
                width: `50px`,
                height: `50px`,
                backgroundImage: `url(${goatImage})`,
                backgroundSize: `50px 50px`
            }
        } else {
            return {
                display: `inline-block`,
                width: `50px`,
                height: `50px`
            }
        }
    }

    render() {
        return <center><div>
            <p><b>Вы играете за:</b> <img class="piece-image" src={this.getImage(this.state.piece)}/><b>, 
                сейчас ходит:</b> <img className="piece-image" src={this.getMoveImage(this.state.currentTurn)}/></p>
            <button class="field-button-queen" onClick={() => this.onClick(0, 0)} style={this.getStyle(this.state.pieces[0][0])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(0, 1)} style={this.getStyle(this.state.pieces[0][1])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(0, 2)} style={this.getStyle(this.state.pieces[0][2])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(0, 3)} style={this.getStyle(this.state.pieces[0][3])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(0, 4)} style={this.getStyle(this.state.pieces[0][4])}></button>
            <br/>
            <button class="field-button-cross" onClick={() => this.onClick(1, 0)} style={this.getStyle(this.state.pieces[1][0])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(1, 1)} style={this.getStyle(this.state.pieces[1][1])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(1, 2)} style={this.getStyle(this.state.pieces[1][2])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(1, 3)} style={this.getStyle(this.state.pieces[1][3])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(1, 4)} style={this.getStyle(this.state.pieces[1][4])}></button>
            <br/>
            <button class="field-button-queen" onClick={() => this.onClick(2, 0)} style={this.getStyle(this.state.pieces[2][0])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(2, 1)} style={this.getStyle(this.state.pieces[2][1])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(2, 2)} style={this.getStyle(this.state.pieces[2][2])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(2, 3)} style={this.getStyle(this.state.pieces[2][3])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(2, 4)} style={this.getStyle(this.state.pieces[2][4])}></button>
            <br/>
            <button class="field-button-cross" onClick={() => this.onClick(3, 0)} style={this.getStyle(this.state.pieces[3][0])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(3, 1)} style={this.getStyle(this.state.pieces[3][1])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(3, 2)} style={this.getStyle(this.state.pieces[3][2])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(3, 3)} style={this.getStyle(this.state.pieces[3][3])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(3, 4)} style={this.getStyle(this.state.pieces[3][4])}></button>
            <br/>
            <button class="field-button-queen" onClick={() => this.onClick(4, 0)} style={this.getStyle(this.state.pieces[4][0])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(4, 1)} style={this.getStyle(this.state.pieces[4][1])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(4, 2)} style={this.getStyle(this.state.pieces[4][2])}></button>
            <button class="field-button-cross" onClick={() => this.onClick(4, 3)} style={this.getStyle(this.state.pieces[4][3])}></button>
            <button class="field-button-queen" onClick={() => this.onClick(4, 4)} style={this.getStyle(this.state.pieces[4][4])}></button>
            <p><b>Коз в загоне: {this.state.spareGoats}, коз на доске: {this.state.boardGoats}</b></p>
            {this.state.winner !== null &&
                <dialog open class="center">
                    <img className="piece-image" src={this.getImage(this.state.winner)}/> <b>выиграли!</b>
                    
                    <form method="dialog">
                        <button class="okay-button">
                            Окей
                        </button>
                    </form>
                </dialog>}
        </div></center>
    }
}