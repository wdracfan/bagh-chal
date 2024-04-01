// import './App.css';
import { User} from "./User";
//import history from 'services/History'
import { BrowserRouter as Router, Switch, Route } from 'react-router-dom';
import {Board} from "./Board";

function App() {
  return (
    <div className="App">
        <Router>
            <Switch>
                <Route exact path="/" component={User} />
                <Route path="/board" component={Board} />
            </Switch>
        </Router>
    </div>
  );
}

export default App;
