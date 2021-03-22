import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter, Route, Switch, Redirect } from "react-router-dom";
import './index.css';
import reportWebVitals from './reportWebVitals';
import Dashboard from './views/Dashboard'
import  'bootstrap/dist/css/bootstrap.min.css' ;

ReactDOM.render(
  <React.StrictMode>
    <BrowserRouter>
        <Switch>
          <Route path="/dashboard" render={(props) => <Dashboard {...props} />} />
          <Redirect from="/" to="/dashboard" />
        </Switch>
      </BrowserRouter>
  </React.StrictMode>,
  document.getElementById('root')
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
