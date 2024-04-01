// соединить в одну сущность: user, board, opponent
import {Component} from "react";
import {withRouter} from "react-router-dom";
import hubConnection from "./WebSocket";
import "./User.css"

export class User extends Component {
    constructor(props) {
        super(props);
        this.state = {
            userId: null,
            boardId: null,
            opponent: null,
            username: null,
            errors: null
        };
    }
    async register(username) {
        const url = "http://localhost:5294/api/register";
        let result;
        await fetch(url, {
            mode: 'cors',
            method: 'POST',
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({"name": username})
        }).then(async res => {
            result = (await res.text()).slice(1, -1);
            console.log(result);
            this.setState({ userId: result });
        });
        return result;
    }
    async createBoard(userId, username, piece) {
        if (!hubConnection.connectionId) {
            await hubConnection.start();
            // подумать, не надо ли каждый раз закрывать коннекшн и открывать заново
        }
        let connectionId = hubConnection.connectionId;
        console.log(connectionId);
        const url = "http://localhost:5294/api/create";
        let result;
        await fetch(url, {
            mode: 'cors',
            method: 'POST',
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                "guid": userId.valueOf(), 
                "connectionId": connectionId,
                "piece": piece
            })
        }).then(async res => {
            result = (await res.text()).slice(1, -1);
            console.log(result);
            this.setState({ boardId: result });
            this.setState({ opponent: null });
        });
        hubConnection.on("opponentJoined", async message => {
            await this.setState({ opponent: message });
            console.log(hubConnection);
            this.props.history.push({
                pathname: "/board",
                state: {
                    userId: userId,
                    username: username,
                    board: result,
                    opponent: message,
                    piece: piece,
                    currentTurn: piece === "goat" // ТУТ ЛОГИКА ВЫБОРА СТОРОНЫ
                }
            });
        });
        return result;
    }
    async joinBoard(userId, boardId) {
        if (!hubConnection.connectionId) {
            await hubConnection.start();
        }
        let connectionId = hubConnection.connectionId;
        console.log(hubConnection.connectionId);
        const url = "http://localhost:5294/api/join";
        console.log(JSON.stringify([this.boardId, this.userId]));
        let result;
        await fetch(url, {
            mode: 'cors',
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                "userId": userId, 
                "boardId": boardId,
                "connectionId": connectionId
            })
        }).then(async res => {
            if (!res.ok) {
                throw new Error(await res.text())
            } else {
                result = await res.json();
                this.setState({ opponent: result.hostName });
            }
        }).catch(e => {
            console.log(e);
            if (e.toString().length > 50) {
                this.setState({errors: "Error: неверный формат Id доски!"});
            } else {
                this.setState({errors: e.toString()});
            }
            result = { hostName: "", piece: "ERROR" };
        });
        return [result.hostName, result.piece];
    }
    async onClickCreate(piece) {
        let username = document.getElementById("username").value;
        this.setState({ username: username });
        let userId = await this.register(username);
        //console.log(userId);
        let boardId = await this.createBoard(userId, username, piece);
    }
    async onClickJoin() {
        this.setState({errors: null});
        console.log(document.getElementById("boardId").value);
        let boardId = document.getElementById("boardId").value;
        let username = document.getElementById("username").value;
        this.setState({username: username});
        this.setState({boardId: boardId});
        let userId = await this.register(username);
        let [opponent, piece] = await this.joinBoard(userId, boardId);
        console.log(opponent, piece);
        if (piece !== "ERROR") {
            this.props.history.push({
                pathname: "/board",
                state: {
                    userId: userId,
                    username: username,
                    board: boardId,
                    opponent: opponent,
                    piece: piece,
                    currentTurn: piece === "goat"
                }
            });
        } else {
            this.setState({boardId: null});
        }
    }
    async onClickBot(piece) { // piece либо "tiger", либо "goat"
        let username = document.getElementById("username").value;
        this.setState({ username: username });
        let userId = await this.register(username);
        //console.log(userId);
        let boardId = await this.createBoard(userId, username);
        this.props.history.push({
            pathname: "/board",
            state: {
                userId: userId,
                username: username,
                board: boardId,
                opponent: null,
                piece: piece,
                currentTurn: piece === "goat"
            }
        });
    }
    render() {
        return (
            <center><div class="center">
                <input id="username" type="text" placeholder="Ваше имя" />
                <p>
                    <button class="user-button" onClick={() => this.onClickCreate("goat")}>Создать доску как коза</button>
                    <button class="user-button" onClick={() => this.onClickCreate("tiger")}>Создать доску как тигр</button>
                </p>
                {!this.state.boardId && 
                <p>
                    <input id="boardId" type="text" placeholder="Id доски"/>
                    <button class="user-button" onClick={() => this.onClickJoin()}>Присоединиться к игре</button>
                </p>}
                {!this.state.boardId && <p>
                    <button class="user-button" onClick={() => this.onClickBot("goat")}>Играть с ботом как коза</button>
                    <button class="user-button" onClick={() => this.onClickBot("tiger")}>Играть с ботом как тигр</button>
                </p>}
                {this.state.boardId && 
                    <p><b>Доска:</b> {this.state.boardId}. Ждём, пока кто-то присоединится...</p>}
                {this.state.errors &&
                    <dialog open class="error center">
                        {this.state.errors}
                        <form method="dialog">
                            <button class="error-button">
                                Окей
                            </button>
                        </form>
                    </dialog>}
            </div></center>
        );
    }
}

export const UserWithRouter = withRouter(User);